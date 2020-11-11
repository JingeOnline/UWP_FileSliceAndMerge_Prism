using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace UWP_FileSliceAndMerge_Prism.Models
{
    public class FileInfoModel:INotifyPropertyChanged
    {
        public string SourceFileName { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileSizeText
        {
            //每三位数添加一个逗号
            get { return FileSize.ToString("N0"); }
        }

        private bool _isStart;
        public bool IsStart
        {
            get { return _isStart; }
            set { _isStart = value;OnPropertyChanged("IsStart"); }
        }
        
        private bool _isDone;
        public bool IsDone 
        {
            get { return _isDone; }
            set
            {
                _isDone = value;
                OnPropertyChanged("IsDone");
                if (IsDone)
                {
                    Icon= "\xF78C";
                }
                else
                {
                    Icon= "\xE8FF";
                }
            }
        }

        private long _finishSize=0;
        public long FinishSize
        {
            get { return _finishSize; }
            set
            {
                _finishSize = value;
                OnPropertyChanged("FinishSize");
            }
        }

        private string _icon;
        public string Icon
        {
            get { return _icon; }
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged("Icon");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propName = "")
        {
            if (this.PropertyChanged != null)
            {
                var handler = PropertyChanged;
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
