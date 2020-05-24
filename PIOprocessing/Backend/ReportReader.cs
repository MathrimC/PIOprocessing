using System.Collections.Generic;
using System.IO;
using System;
using Caliburn.Micro;
using System.Windows;

namespace PIOprocessing {
    class ReportReader
    {
        protected bool isLoaded;
        protected string path;
        protected List<Report> reports;
        protected List<Report> unprocessedReports;
        public List<Report> UnprocessedReports { get { return unprocessedReports; } }

        protected HashSet<string> actionList;
        protected SortedDictionary<string, HashSet<string>> aggPosLists;
        protected SortedDictionary<string, HashSet<string>> cllPosLists;
        protected SortedDictionary<string, HashSet<string>> boardTypeLists;
        protected SortedDictionary<string, HashSet<string>> boardSubtypeLists;
        protected SortedDictionary<string, Report> reportList;
        public HashSet<string> GetActionList()
        {
            return actionList;
        }
        public HashSet<string> GetAggPosList(string action)
        {
            return aggPosLists.GetValueOrDefault(action);
        }
        public HashSet<string> GetCllPosList(string action, string aggPos)
        {
            return cllPosLists.GetValueOrDefault(action + aggPos);
        }
        public HashSet<string> GetBoardTypeList(string action, string aggPos, string cllPos)
        {
            return boardTypeLists.GetValueOrDefault(action + aggPos + cllPos);
        }
        public HashSet<string> GetSubtypeList(string action, string aggPos, string cllPos, string boardType)
        {
            return boardSubtypeLists[action + aggPos + cllPos + boardType];
        }
        public Report GetReport (string action, string aggPos, string cllPos, string boardType, string boardSubtype)
        {
            return reportList[action + aggPos + cllPos + boardType + boardSubtype];
        }

        public string Path {get {return path;}}
        public List<Report> Reports {get {return reports;}}
        public ReportReader(string path)
        {
            staticTimer.start("ReportScanning");
            this.path = path;
            isLoaded = false;
            reports = new List<Report>();
            unprocessedReports = new List<Report>();
            actionList = new HashSet<string>();
            aggPosLists = new SortedDictionary<string,HashSet<string>>();
            cllPosLists = new SortedDictionary<string,HashSet<string>>();
            boardTypeLists = new SortedDictionary<string,HashSet<string>>();
            boardSubtypeLists = new SortedDictionary<string,HashSet<string>>();
            reportList = new SortedDictionary<string,Report>();
            loadReportFiles();
            staticTimer.stop("ReportScanning");
            staticTimer.log("ReportScanning");
        }

        public void Refresh()
        {
            loadReportFiles();
        }

        protected void loadReportFiles() {
            loadReportFiles(path);
        }

        protected void loadReportFiles(string subpath) {
            string[] subDirectories = Directory.GetDirectories(subpath);
            foreach(string subDirectory in subDirectories) {
                loadReportFiles(subDirectory);
            }
            string[] filePaths = Directory.GetFiles(subpath);
            int stacksize = 0;
            List<Report> tmpReportList = new List<Report>();
            foreach(string filePath in filePaths) {
                if(isFrequencyReport(filePath)) {
                    Report report = new Report(filePath);
                    if (report.ResolvedSpot)
                    {
                        reports.Add(report);
                        tmpReportList.Add(report);
                        addToSpotTree(report);
                    } else
                    {
                        unprocessedReports.Add(report);
                    }
                } else {
                    int size = tryGetStackSize(filePath);
                    if(size > 0)
                    {
                        stacksize = size;
                    }
                }
            }
            foreach(Report report in tmpReportList)
            {
                report.Stacksize = stacksize;
            }
        }

        protected void addToSpotTree(Report report) {
            
            string action = report.Spot.Action, aggPos = report.Spot.AggPos, cllPos = report.Spot.CllPos, boardType = report.Spot.BoardType, boardSubtype =  report.Spot.BoardSubtype;
            
            string index = action;

            if(!actionList.Contains(index))
            {
                actionList.Add(index);
            }
            if(!aggPosLists.ContainsKey(index)) {
                aggPosLists.Add(index,new HashSet<string>());
            }
            if(aggPosLists[index].Add(aggPos)) {
                cllPosLists.Add(index + aggPos,new HashSet<string>());
            }
            index += aggPos;
            if(cllPosLists.GetValueOrDefault(index).Add(cllPos)) {
                boardTypeLists.Add(index + cllPos, new HashSet<string>());
            }
            index += cllPos;
            if(boardTypeLists.GetValueOrDefault(index).Add(boardType)) {
                boardSubtypeLists.Add(index + boardType, new HashSet<string>());
            }
            index += boardType;

            if (boardSubtype != "")
            {
                boardSubtypeLists.GetValueOrDefault(index).Add(boardSubtype);
            }

            if(!reportList.ContainsKey(index + boardSubtype))
                reportList.Add(index + boardSubtype, report);

        }

        // If the file is a PIO info file, it extracts the stacksize, otherwise it returns 0
        protected int tryGetStackSize(string filePath)
        {
            if(!filePath.EndsWith("info.txt")) {
                return 0;
            }

            using (var file = new FileStream(@filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(file))
            {

                int lineIndex = 0;
                while (lineIndex < 4 && !reader.EndOfStream)
                {
                    reader.ReadLine();
                    lineIndex++;
                }
                if(!reader.EndOfStream)
                {
                    
                    string line = reader.ReadLine();
                    string[] values = line.Split(' ');
                    return int.Parse(values[2]);
                } else
                {
                    return 0;
                }
            }
        }
        
        protected bool isFrequencyReport(string filePath) {
            if(!filePath.EndsWith("_Full.csv")) {
                return false;
            }
            using (var file = new FileStream (@filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using(var reader = new StreamReader(file))
            {
                if(!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    return Array.Exists(values, header => header.EndsWith("Freq"));
                } else {
                    return false;
                }
            }
        }


    }
}