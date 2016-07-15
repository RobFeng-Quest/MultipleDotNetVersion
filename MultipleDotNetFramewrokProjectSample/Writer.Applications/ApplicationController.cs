using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Waf.Applications;
using Writer.Applications.Documents;
using Writer.Applications.Properties;
using Writer.Applications.Services;
using Writer.Applications.ViewModels;
using Writer.Applications.Views;

namespace Writer.Applications.Controllers
{
    [Export]
    public class ApplicationController : Controller
    {
        private readonly CompositionContainer container;
        private readonly IDocumentManager documentManager;
        private readonly RichTextDocumentController richTextDocumentController;
        private readonly ShellViewModel shellViewModel;
        private readonly MainViewModel mainViewModel;
        private readonly DelegateCommand exitCommand;
        private RichTextDocumentType richTextDocumentType;

        
        [ImportingConstructor]
        public ApplicationController(CompositionContainer container, IPresentationController presentationController, 
            IDocumentManager documentManager)
        {
            if (container == null) { throw new ArgumentNullException("container"); }
            if (presentationController == null) { throw new ArgumentNullException("presentationController"); }
            if (documentManager == null) { throw new ArgumentNullException("documentManager"); }
            
            InitializeCultures();
            presentationController.InitializeCultures();

            this.container = container;
            this.documentManager = documentManager;
            this.documentManager.DocumentsClosing += DocumentManagerDocumentsClosing;

            this.richTextDocumentController = container.GetExportedValue<RichTextDocumentController>();
            this.shellViewModel = container.GetExportedValue<ShellViewModel>();
            this.mainViewModel = container.GetExportedValue<MainViewModel>();
            
            this.shellViewModel.Closing += ShellViewModelClosing;
            this.exitCommand = new DelegateCommand(Close);
        }

        
        public void Initialize()
        {
            mainViewModel.ExitCommand = exitCommand;

            richTextDocumentType = new RichTextDocumentType();
            documentManager.Register(richTextDocumentType);
            documentManager.Register(new XpsExportDocumentType());
        }

        public void Run()
        {
            shellViewModel.ContentView = mainViewModel.View;
            documentManager.New(richTextDocumentType);
            shellViewModel.Show();
        }

        public void Shutdown()
        {
            if (mainViewModel.NewLanguage != null) 
            { 
                Settings.Default.UICulture = mainViewModel.NewLanguage.Name; 
            }
            Settings.Default.Save();
        }

        private static void InitializeCultures()
        {
            if (!String.IsNullOrEmpty(Settings.Default.Culture))
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.Culture);
            }
            if (!String.IsNullOrEmpty(Settings.Default.UICulture))
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.UICulture);
            }
        }

        private void DocumentManagerDocumentsClosing(object sender, DocumentsClosingEventArgs e)
        {
            IEnumerable<IDocument> modifiedDocuments = e.Documents.Where(d => d.Modified).ToList();
            if (!modifiedDocuments.Any()) { return; }
        }

        private void Close()
        {
            shellViewModel.Close();
        }

        private void ShellViewModelClosing(object sender, CancelEventArgs e)
        {
            // Try to close all documents and see if the user has already saved them.
            e.Cancel = !documentManager.CloseAll();
        }
    }
}
