using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UWP_FileSliceAndMerge_Prism.Models;

namespace UWP_FileSliceAndMerge_Prism.Services.TxtFile
{
    public class TxtInfoService
    {
        IEnumerable<TxtEntiretyInfoModel> _txtFiles;
        bool _isChinese;
        public TxtInfoService(IEnumerable<TxtEntiretyInfoModel> txtFiles,bool isChinese)
        {
            _txtFiles = txtFiles;
            _isChinese = isChinese;
        }

        public async Task FindWordCountAndLineCount()
        {
            foreach(TxtEntiretyInfoModel txt in _txtFiles)
            {
                await findTxtWordCount(txt);
                Debug.WriteLine("计算所在线程3：" + Thread.CurrentThread.ManagedThreadId);
            }
        }

        private async Task findTxtWordCount(TxtEntiretyInfoModel txt)
        {
            //string runtimeLanguages = Windows.ApplicationModel.Resources.Core
            //    .ResourceContext.GetForCurrentView().QualifierValues["Language"];
            //Debug.WriteLine(runtimeLanguages);
            Debug.WriteLine("计算所在线程1："+Thread.CurrentThread.ManagedThreadId);
            string content= await Windows.Storage.FileIO.ReadTextAsync(txt.StorageFile);
            Debug.WriteLine("计算所在线程2：" + Thread.CurrentThread.ManagedThreadId);
            txt.TextContent = content;
            //查找字数
            if (_isChinese)
            {
                txt.TxtWordCount = content.Length;
                Debug.WriteLine("中文字数：" + content.Length);
            }
            else
            {
                char[] delimiters = new char[] { ' ', '\r', '\n' };
                long wordCount = content.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
                txt.TxtWordCount = wordCount;
                Debug.WriteLine("西文字数："+wordCount);
            }
            //查找行数
            if (string.IsNullOrEmpty(content))
            {
                txt.TxtLineCount = 0;
            }
            else
            {
                txt.TxtLineCount = content.Split('\n').Length;
                Debug.WriteLine("行数：" + txt.TxtLineCount);
            }

        }
    }
}
