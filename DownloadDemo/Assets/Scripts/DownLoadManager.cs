using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;


/// <summary>
/// 下载管理器
/// </summary>
public class DownLoadManager: MonoBehaviour
{
    // 下载线程的数量
    private const int THREAD_COUNT = 8;
    private const int HTTP_TIMEOUT = 5000;
    private const int READ_TIMEOUT = 30000;
    private const int TRY_COUNT = 10;
    private const int SPEED_DELTA = 500;
    private const int BUFFER_SIZE = 64 * 1024;
    // 等待的任务队列
    private Queue<DownLoadTask> _taskQueue;
    // 完成队列
    private Queue<DownLoadTask> _doneQueue;
    // 当前下载的任务列表
    private List<DownLoadTask> _workList;
    // 失败的任务列表
    private List<DownLoadTask> _failedList;
    // 缓存列表
    private List<byte[]> _bufferList;
    // 执行下载任务的主线程
    private Thread _workThread;
    private AutoResetEvent _workThreadEvent;
    private bool _needStop = false;
    private object _object = new object();
    private bool _isPause = false;
    private bool _isFailed = false;
    private bool _isContinue = false;
    private long _speed = 0;
    private long _speedTime = 0;
    private long _deltaSize = 0;
    private long _downDoneSize = 0;
    private long _downLoadSize = 0;
    private static DownLoadManager s_instance = null;
    public static DownLoadManager GetInstance()
    {
        if(s_instance == null)
        {
            GameObject insObj = new GameObject();
            GameObject.DontDestroyOnLoad(insObj);
            s_instance = insObj.AddComponent<DownLoadManager>();
        }
        return s_instance;
    }
    private void Awake()
    {
        ServicePointManager.DefaultConnectionLimit = 512;
        ServicePointManager.MaxServicePoints = 512;
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)=> { return true; });
        _taskQueue = new Queue<DownLoadTask>();
        _doneQueue = new Queue<DownLoadTask>();
        _workList = new List<DownLoadTask>(THREAD_COUNT);
        _failedList = new List<DownLoadTask>(THREAD_COUNT);
        _bufferList = new List<byte[]>(THREAD_COUNT);
        for(int i = 0; i < THREAD_COUNT; ++i)
        {
            _workList.Add(null);
            _bufferList.Add(null);
        }
        _workThreadEvent = new AutoResetEvent(false);
        _workThread = new Thread(WorkThreadFunc);
        _workThread.Start();
    }

    private void OnDestroy()
    {
        StopThread(false);
    }

    private void Update()
    {
        while(_doneQueue.Count > 0)
        {
            DownLoadTask task = _doneQueue.Dequeue();
            if(task.callBack != null)
            {
                task.callBack(task);
            }
        }
    }

    /// <summary>
    /// 停止工作线程
    /// </summary>
    /// <param name="waitForEnd"></param>
    private void StopThread(bool waitForEnd)
    {
        if(_workThread == null)
        {
            return;
        }
        _needStop = true;
        Clear();
        Resume();
        if(waitForEnd && _workThread.IsAlive)
        {
            _workThread.Join();
        }
        _workThread = null;
    }

    /// <summary>
    /// 重置下载大小
    /// </summary>
    /// <param name="doneSize"></param>
    public void Reset(long doneSize)
    {
        Clear();
        lock (_object)
        {
            _downDoneSize = doneSize;
        }
    }

    /// <summary>
    /// 清空所有任务
    /// </summary>
    public void Clear()
    {
        lock(_object)
        {
            _taskQueue.Clear();
            _failedList.Clear();
            for(int i = 0; i < _workList.Count; ++i)
            {
                if(_workList[i] != null)
                {
                    _workList[i].isCancel = true;
                    _workList[i] = null;
                }
                _bufferList[i] = null;
            }
            _isPause = false;
            _isFailed = false;
            _isContinue = false;
            _speed = 0;
            _speedTime = 0;
            _deltaSize = 0;
            _downDoneSize = 0;
            _downLoadSize = 0;
        }
    }


    /// <summary>
    /// 获取下载速度
    /// </summary>
    /// <returns></returns>
    public long GetSpeed()
    {
        return _speed;
    }

    /// <summary>
    /// 是否暂停
    /// </summary>
    /// <returns></returns>
    public bool IsPause()
    {
        return _isPause;
    }

    /// <summary>
    /// 是否失败
    /// </summary>
    /// <returns></returns>
    public bool IsFaied()
    {
        return _isFailed;
    }

    /// <summary>
    /// 获取当前下载的大小
    /// </summary>
    /// <returns></returns>
    public long GetDownLoadSize()
    {
        return _downDoneSize + _downLoadSize;
    }

    /// <summary>
    /// 继续失败任务
    /// </summary>
    public void Continue()
    {
        if (_isPause)
        {
            _isPause = false;
            _isContinue = true;
            _workThreadEvent.Set();
        }
    }

    /// <summary>
    /// 恢复工作线程
    /// </summary>
    private void Resume()
    {
        if (_isPause)
        {
            _isPause = false;
            _isContinue = false;
            _workThreadEvent.Set();
        }
    }

    /// <summary>
    /// 暂停工作线程
    /// </summary>
    private void Pause(bool isFailed)
    {
        if (!_isPause)
        {
            _isPause = true;
            _isFailed = isFailed;
            _workThreadEvent.WaitOne();
        }
    }

    /// <summary>
    /// 计算下载速度
    /// </summary>
    /// <param name="size"></param>
    private void CalcSpeed(long size)
    {
        if (_speedTime == 0)
        {
            _deltaSize = size;
            _speedTime = System.DateTime.Now.Ticks;
            return;
        }
        long deltaTime = System.DateTime.Now.Ticks - _speedTime;
        if (deltaTime > SPEED_DELTA * 10000)
        {
            _speed = ((size - _deltaSize) * 1000 * 10000) / deltaTime;
            _speed = _speed < 0 ? 0 : _speed;
            _deltaSize = size;
            _speedTime = System.DateTime.Now.Ticks;
        }
    }

    /// <summary>
    /// 获取空闲的任务索引
    /// </summary>
    /// <returns></returns>
    private int GetWorkIndex()
    {
        for(int i = 0; i < _workList.Count; ++i)
        {
            if(_workList[i] == null)
            {
                return i;
            } 
        }
        return -1;
    }

    /// <summary>
    /// 线程池执行任务
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    private bool DoWorkTaskInThreadPool(DownLoadTask task)
    {
        int workIndex = GetWorkIndex();
        if(workIndex != -1 && task != null)
        {
            task.isDone = false;
            _workList[workIndex] = task;
            ThreadPool.QueueUserWorkItem(ThreadPoolFunc, workIndex);
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// 恢复失败任务
    /// </summary>
    private void ResumeFailedList(bool force = false)
    {
        if(!force && (!_isContinue || !_isFailed))
        {
            return;
        }
        for (int i = 0; i < _failedList.Count; ++i)
        {
            if (_failedList[i] != null)
            {
                DoWorkTaskInThreadPool(_failedList[i]);
            }
        }
        _isFailed = false;
    }

    /// <summary>
    /// 是否有等待任务
    /// </summary>
    /// <returns></returns>
    private bool IsWorkWait()
    {
        return _taskQueue.Count > 0;
    }

    /// <summary>
    /// 是否有失败的任务
    /// </summary>
    /// <returns></returns>
    private bool IsWorkFailed()
    {
        return _failedList.Count > 0;
    }

    /// <summary>
    /// 是否正在执行任务
    /// </summary>
    /// <returns></returns>
    private bool IsWorkDoing()
    {
        long doingSize = 0;
        long doneSize = 0;
        bool isDoing = false;
        for (int i = 0; i < _workList.Count; ++i)
        {
            DownLoadTask task = _workList[i];
            if (task != null)
            {
                if (task.isDone)
                {
                    _workList[i] = null;
                    if (task.isSuccess)
                    {
                        doneSize += task.downLoadSize;
                    }
                    else
                    {
                        _failedList.Add(task);
                    }
                    _doneQueue.Enqueue(task);
                }
                else
                {
                    doingSize += task.downLoadSize;
                    isDoing = true;
                }
            }
        }
        _downLoadSize = doingSize;
        _downDoneSize += doneSize;

        CalcSpeed(GetDownLoadSize());

        return isDoing;
    }

    /// <summary>
    /// 工作线程函数
    /// </summary>
    private void WorkThreadFunc()
    {
        do
        {
            bool isPause = false;
            bool isFailed = false;
            lock (_object)
            {
                ResumeFailedList();
                long lastDoneSize = _downDoneSize;
                bool isWorkDoing = IsWorkDoing();
                bool isWorkFailed = IsWorkFailed();
                bool isWorkWait = IsWorkWait();
                if (!isWorkWait && !isWorkDoing && !isWorkFailed)
                {
                    isPause = true;
                }
                else
                {
                    if (isWorkFailed)
                    {
                        // 如果有新成功的任务，则恢复失败的任务
                        if(_downDoneSize > lastDoneSize)
                        {
                            ResumeFailedList(true);
                        }
                        else
                        {
                            if (!isWorkDoing)
                            {
                                isPause = true;
                                isFailed = true;
                            }
                        }
                    }
                    else if (isWorkWait)
                    {
                        bool result = DoWorkTaskInThreadPool(_taskQueue.Peek());
                        if (result)
                        {
                            _taskQueue.Dequeue();
                        }
                    }
                }
            }

            if(isPause)
            {
                Pause(isFailed);
            }
            else
            {
                Thread.Sleep(50);
            }
        } while (!_needStop);
    }

    /// <summary>
    /// 执行任务的线程池函数
    /// </summary>
    /// <param name="param"></param>
    private void ThreadPoolFunc(object param)
    {
        DownLoadTask task = null;
        lock(_object)
        {
            int workIndex = (int)param;
            task = _workList[workIndex];
            if(task != null)
            {
                task.buffer = _bufferList[workIndex];
                if(task.buffer == null)
                {
                    task.buffer = new byte[BUFFER_SIZE];
                    _bufferList[workIndex] = task.buffer;
                }
            }
        }
        if(task != null)
        {
            DoDownLoadTask(task);
        }
    }

    /// <summary>
    /// 执行下载任务
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    private bool DoDownLoadTask(DownLoadTask task)
    {
        if(task == null || task.isDone)
        {
            return false;
        }
        int tryCount = 0;
        HttpWebRequest httpWebRequest = null;
        HttpWebResponse webResponse = null;
        FileStream fs = null;
        Stream responseStream = null;
        Byte[] bytes = task.buffer;
        bool isSuccess = false;
        bool timeOut = false;
        do
        {
            if (task.isCancel)
            {
                break;
            }
            try
            {
                if (task.totalSize <= 0)
                {
                    task.totalSize = GetFileLength(task.url);
                }
                httpWebRequest = CreateWebRequest(task.url);
                long range = 0;
                fs = new FileStream(task.path, FileMode.OpenOrCreate);
                if (fs.Length > 0 && task.totalSize > fs.Length && fs.Length < int.MaxValue)
                {
                    long pos = fs.Seek(0, SeekOrigin.End);
                    task.downLoadSize = pos;
                    range += pos;
                }
                else
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                    task.downLoadSize = 0;
                }
                httpWebRequest.AddRange((int)range);
                webResponse = httpWebRequest.GetResponse() as HttpWebResponse;
                if (webResponse.StatusCode == HttpStatusCode.OK || webResponse.StatusCode == HttpStatusCode.PartialContent)
                {
                    responseStream = webResponse.GetResponseStream();
                    responseStream.ReadTimeout = READ_TIMEOUT;
                    int size = 0;
                    while (!task.IsDownLoadDone())
                    {
                        if (task.isCancel)
                        {
                            break;
                        }
                        size = responseStream.Read(bytes, 0, bytes.Length);
                        if (size > 0)
                        {
                            fs.Write(bytes, 0, size);
                            task.downLoadSize += size;
                        }
                        else
                        {
                            break;
                        }
                    }
                    responseStream.Close();
                    responseStream = null;
                    webResponse.Close();
                    webResponse = null;
                    httpWebRequest.Abort();
                    httpWebRequest = null;
                    fs.Close();
                    fs = null;
                    if(task.IsDownLoadDone())
                    {
                        isSuccess = true;
                    }
                }
                else
                {
                    Debug.LogErrorFormat("webResponse code:{0}", webResponse.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                System.Net.WebException webException = ex as WebException;
                if(webException != null && webException.Status == WebExceptionStatus.Timeout && webException.TargetSite.Name == "Read")
                {
                    //timeOut = true;
                }
                else
                {
                    timeOut = false;
                }
                Debug.LogError(ex.StackTrace);
                Debug.LogError(ex.Message);
                if (responseStream != null)
                {
                    responseStream.Close();
                    responseStream = null;
                }
                if (webResponse != null)
                {
                    webResponse.Close();
                    webResponse = null;
                }
                if (httpWebRequest != null)
                {
                    httpWebRequest.Abort();
                    httpWebRequest = null;
                }
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
            }

            if(isSuccess)
            {
                break;
            }
            else
            {
                if(!timeOut)
                {
                    tryCount++;
                }
                Thread.Sleep(50);
            }

        } while (tryCount < TRY_COUNT);

        task.Done(isSuccess);

        return isSuccess;
    }

    /// <summary>
    /// 创建HTTP请求
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private HttpWebRequest CreateWebRequest(string url)
    {
        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        {
            request.ProtocolVersion = HttpVersion.Version10;
        }
        request.AllowAutoRedirect = true;
        request.Timeout = HTTP_TIMEOUT;
        request.Method = "GET";
        request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
        request.KeepAlive = true;
        request.CookieContainer = null;
        request.UseDefaultCredentials = true;
        request.ServicePoint.ConnectionLimit = 512;
        
        /*
        IPAddress[] ips = Dns.GetHostAddresses("woda.jijiagames.com");
        if (ips != null)
        {
            request.Proxy = new WebProxy("http://" + ips[0].ToString());
            for (int i = 0; i < ips.Length; ++i)
            {
                Debug.Log("ip:" + ips[i].ToString());
            }
        }
        else
        {
            Debug.LogError("GetHostAddresses Failed!");
        }
        */

        return request;
    }

    /// <summary>
    /// 获取下载文件的大小
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private long GetFileLength(string url)
    {
        long size = -1;
        HttpWebResponse webResponse = null;
        HttpWebRequest webRequest = null;
        try
        {
            webRequest = CreateWebRequest(url);
            webResponse = webRequest.GetResponse() as HttpWebResponse;        
            if (webResponse != null)
            {
                size = webResponse.ContentLength;
                webResponse.Close();
                webResponse = null;
            }
            webRequest.Abort();
            webRequest = null;

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            if (webResponse != null)
            {
                webResponse.Close();
                webResponse = null;
            }
            if (webRequest != null)
            {
                webRequest.Abort();
                webRequest = null;
            }
        }
        return size;
    }

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="downLoadTask"></param>
    /// <returns></returns>
    public DownLoadTask DoTask(DownLoadTask downLoadTask)
    {
        lock (_object)
        {
            _taskQueue.Enqueue(downLoadTask);
        }
        Resume();
        return downLoadTask;
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="url"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public DownLoadTask DownLoad(string url, string file)
    {
        DownLoadTask downLoadTask = new DownLoadTask();
        downLoadTask.url = url;
        downLoadTask.path = file;
        DoTask(downLoadTask);
        return downLoadTask;
    }


}
