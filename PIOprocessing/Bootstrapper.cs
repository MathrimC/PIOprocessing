using Caliburn.Micro;
using PIOprocessing.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PIOprocessing
{
    public class Bootstrapper : BootstrapperBase
    {
        // private readonly SimpleContainer container = new SimpleContainer();

        public Bootstrapper()
        {
            Initialize();
        }

        /*
        protected override void Configure()
        {
            container.Singleton<IEventAggregator, EventAggregator>();
            base.Configure();
        }
        */

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
