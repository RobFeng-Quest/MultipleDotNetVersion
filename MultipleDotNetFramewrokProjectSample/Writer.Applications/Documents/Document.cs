﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Waf.Foundation;

namespace Writer.Applications.Documents
{
    public abstract class Document : Model, IDocument
    {
        private readonly IDocumentType documentType;
        private string fileName;
        private bool modified;


        protected Document(IDocumentType documentType)
        {
            if (documentType == null) { throw new ArgumentNullException("documentType"); }
            this.documentType = documentType;
        }


        public IDocumentType DocumentType { get { return documentType; } }

        public string FileName 
        {
            get { return fileName; }
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    RaisePropertyChanged("FileName");
                }
            }
        }

        public bool Modified
        {
            get { return modified; }
            set
            {
                if (modified != value)
                {
                    modified = value;
                    RaisePropertyChanged("Modified");
                }
            }
        }
    }
}
