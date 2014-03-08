using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.SessionState;

namespace Http.TestLibrary
{
    public enum HttpVerb
    {
        GET,
        HEAD,
        POST,
        PUT,
        DELETE,
    }

    /// <summary>
    /// Useful class for simulating the HttpContext. This does not actually
    /// make an HttpRequest, it merely simulates the state that your code
    /// would be in "as if" handling a request. Thus the HttpContext.Current
    /// property is populated.
    /// </summary>
    public class HttpSimulator : IDisposable
    {
        private const string defaultPhysicalAppPath = @"c:\InetPub\wwwRoot\";
        private StringBuilder builder;
        private Uri _referer;
        private NameValueCollection _formVars = new NameValueCollection();
        private NameValueCollection _headers = new NameValueCollection();

        public HttpSimulator()
            : this("/", defaultPhysicalAppPath)
        {
        }

        public HttpSimulator(string applicationPath)
            : this(applicationPath, defaultPhysicalAppPath)
        {
        }

        public HttpSimulator(string applicationPath, string physicalApplicationPath)
        {
            this.ApplicationPath = applicationPath;
            this.PhysicalApplicationPath = physicalApplicationPath;
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a GET request.
        /// </summary>
        /// <remarks>
        /// Simulates a request to http://localhost/
        /// </remarks>
        public HttpSimulator SimulateRequest()
        {
            return SimulateRequest(new Uri("http://localhost/"));
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a GET request.
        /// </summary>
        /// <param name="url"></param>
        public HttpSimulator SimulateRequest(Uri url)
        {
            return SimulateRequest(url, HttpVerb.GET);
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpVerb"></param>
        public HttpSimulator SimulateRequest(Uri url, HttpVerb httpVerb)
        {
            return SimulateRequest(url, httpVerb, null, null);
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a POST request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="formVariables"></param>
        public HttpSimulator SimulateRequest(Uri url, NameValueCollection formVariables)
        {
            return SimulateRequest(url, HttpVerb.POST, formVariables, null);
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a POST request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="formVariables"></param>
        /// <param name="headers"></param>
        public HttpSimulator SimulateRequest(Uri url, NameValueCollection formVariables, NameValueCollection headers)
        {
            return SimulateRequest(url, HttpVerb.POST, formVariables, headers);
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpVerb"></param>
        /// <param name="headers"></param>
        public HttpSimulator SimulateRequest(Uri url, HttpVerb httpVerb, NameValueCollection headers)
        {
            return SimulateRequest(url, httpVerb, null, headers);
        }

        /// <summary>
        /// Sets up the HttpContext objects to simulate a request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpVerb"></param>
        /// <param name="formVariables"></param>
        /// <param name="headers"></param>
        protected virtual HttpSimulator SimulateRequest(Uri url, HttpVerb httpVerb, NameValueCollection formVariables, NameValueCollection headers)
        {
            HttpContext.Current = null;

            ParseRequestUrl(url);

            if (this.responseWriter == null)
            {
                this.builder = new StringBuilder();
                this.responseWriter = new StringWriter(builder);
            }

            SetHttpRuntimeInternals();

            string query = ExtractQueryStringPart(url);

            if (formVariables != null)
                _formVars.Add(formVariables);

            if (_formVars.Count > 0)
                httpVerb = HttpVerb.POST; //Need to enforce this.

            if (headers != null)
                _headers.Add(headers);

            this.workerRequest = new SimulatedHttpRequest(ApplicationPath, PhysicalApplicationPath, PhysicalPath, Page, query, this.responseWriter, host, port, httpVerb.ToString(), url);

            this.workerRequest.Form.Add(_formVars);
            this.workerRequest.Headers.Add(_headers);

            if (_referer != null)
                this.workerRequest.SetReferer(_referer);

            InitializeSession();

            InitializeApplication();

            #region Console Debug INfo

            Console.WriteLine("host: " + host);
            Console.WriteLine("virtualDir: " + applicationPath);
            Console.WriteLine("page: " + localPath);
            Console.WriteLine("pathPartAfterApplicationPart: " + _page);
            Console.WriteLine("appPhysicalDir: " + physicalApplicationPath);
            Console.WriteLine("Request.Url.LocalPath: " + HttpContext.Current.Request.Url.LocalPath);
            Console.WriteLine("Request.Url.Host: " + HttpContext.Current.Request.Url.Host);
            Console.WriteLine("Request.FilePath: " + HttpContext.Current.Request.FilePath);
            Console.WriteLine("Request.Path: " + HttpContext.Current.Request.Path);
            Console.WriteLine("Request.RawUrl: " + HttpContext.Current.Request.RawUrl);
            Console.WriteLine("Request.Url: " + HttpContext.Current.Request.Url);
            Console.WriteLine("Request.Url.Port: " + HttpContext.Current.Request.Url.Port);
            Console.WriteLine("Request.ApplicationPath: " + HttpContext.Current.Request.ApplicationPath);
            Console.WriteLine("Request.PhysicalPath: " + HttpContext.Current.Request.PhysicalPath);
            Console.WriteLine("HttpRuntime.AppDomainAppPath: " + HttpRuntime.AppDomainAppPath);
            Console.WriteLine("HttpRuntime.AppDomainAppVirtualPath: " + HttpRuntime.AppDomainAppVirtualPath);
            Console.WriteLine("HostingEnvironment.ApplicationPhysicalPath: " + HostingEnvironment.ApplicationPhysicalPath);
            Console.WriteLine("HostingEnvironment.ApplicationVirtualPath: " + HostingEnvironment.ApplicationVirtualPath);

            #endregion Console Debug INfo

            return this;
        }

        private static void InitializeApplication()
        {
            Type appFactoryType = Type.GetType("System.Web.HttpApplicationFactory, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            object appFactory = ReflectionHelper.GetStaticFieldValue<object>("_theApplicationFactory", appFactoryType);
            if (appFactory==null)return;
            ReflectionHelper.SetPrivateInstanceFieldValue("_state", appFactory, HttpContext.Current.Application);
        }

        private void InitializeSession()
        {
            HttpContext.Current = new HttpContext(workerRequest);
            HttpContext.Current.Items.Clear();
            var fakeHttpSessionState = new FakeHttpSessionState();
            HttpSessionState session = (HttpSessionState)ReflectionHelper.Instantiate(typeof(HttpSessionState), new Type[] { typeof(IHttpSessionState) }, fakeHttpSessionState);
            Context = new HttpContextWrapper(new WrappedSimulatedHttpRequest(workerRequest), new WrappedFakeHttpSessionState(fakeHttpSessionState));

            HttpContext.Current.Items.Add("AspSession", session);
        }

        /// <summary>
        /// Sets the referer for the request. Uses a fluent interface.
        /// </summary>
        /// <param name="referer"></param>
        /// <returns></returns>
        public HttpSimulator SetReferer(Uri referer)
        {
            if (this.workerRequest != null)
                this.workerRequest.SetReferer(referer);
            this._referer = referer;
            return this;
        }

        /// <summary>
        /// Sets a form variable.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpSimulator SetFormVariable(string name, string value)
        {
            //TODO: Change this ordering requirement.
            if (this.workerRequest != null)
                throw new InvalidOperationException("Cannot set form variables after calling Simulate().");

            _formVars.Add(name, value);

            return this;
        }

        /// <summary>
        /// Sets a header value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpSimulator SetHeader(string name, string value)
        {
            //TODO: Change this ordering requirement.
            if (this.workerRequest != null)
                throw new InvalidOperationException("Cannot set headers after calling Simulate().");

            _headers.Add(name, value);

            return this;
        }

        private void ParseRequestUrl(Uri url)
        {
            if (url == null)
                return;
            this.host = url.Host;
            this.port = url.Port;
            this.localPath = url.LocalPath;
            this._page = StripPrecedingSlashes(RightAfter(url.LocalPath, ApplicationPath));
            this.physicalPath = Path.Combine(this.physicalApplicationPath, this._page.Replace("/", @"\"));
        }

        static string RightAfter(string original, string search)
        {
            if (search.Length > original.Length || search.Length == 0)
                return original;

            int searchIndex = original.IndexOf(search, 0, StringComparison.InvariantCultureIgnoreCase);

            if (searchIndex < 0)
                return original;

            return original.Substring(original.IndexOf(search) + search.Length);
        }

        public string Host
        {
            get { return this.host; }
        }

        private string host;

        public string LocalPath
        {
            get { return this.localPath; }
        }

        private string localPath;

        public int Port
        {
            get { return this.port; }
        }

        private int port;

        /// <summary>
        /// Portion of the URL after the application.
        /// </summary>
        public string Page
        {
            get { return this._page; }
        }

        private string _page;

        /// <summary>
        /// The same thing as the IIS Virtual directory. It's
        /// what gets returned by Request.ApplicationPath.
        /// </summary>
        public string ApplicationPath
        {
            get { return this.applicationPath; }
            set
            {
                this.applicationPath = value ?? "/";
                this.applicationPath = NormalizeSlashes(this.applicationPath);
            }
        }

        private string applicationPath = "/";

        /// <summary>
        /// Physical path to the application (used for simulation purposes).
        /// </summary>
        public string PhysicalApplicationPath
        {
            get { return this.physicalApplicationPath; }
            set
            {
                this.physicalApplicationPath = value ?? defaultPhysicalAppPath;
                //strip trailing backslashes.
                this.physicalApplicationPath = StripTrailingBackSlashes(this.physicalApplicationPath) + @"\";
            }
        }

        private string physicalApplicationPath = defaultPhysicalAppPath;

        /// <summary>
        /// Physical path to the requested file (used for simulation purposes).
        /// </summary>
        public string PhysicalPath
        {
            get { return this.physicalPath; }
        }

        private string physicalPath = defaultPhysicalAppPath;

        public TextWriter ResponseWriter
        {
            get { return this.responseWriter; }
            set { this.responseWriter = value; }
        }

        /// <summary>
        /// Returns the text from the response to the simulated request.
        /// </summary>
        public string ResponseText
        {
            get
            {
                return (builder ?? new StringBuilder()).ToString();
            }
        }

        private TextWriter responseWriter;

        public SimulatedHttpRequest WorkerRequest
        {
            get { return this.workerRequest; }
        }

        public HttpContextBase Context { get; private set; }

        private SimulatedHttpRequest workerRequest;

        private static string ExtractQueryStringPart(Uri url)
        {
            string query = url.Query ?? string.Empty;
            if (query.StartsWith("?"))
                return query.Substring(1);
            return query;
        }

        void SetHttpRuntimeInternals()
        {
            //We cheat by using reflection.

            // get singleton property value
            HttpRuntime runtime = ReflectionHelper.GetStaticFieldValue<HttpRuntime>("_theRuntime", typeof(HttpRuntime));
            if (null == runtime) // 
                return;
            // set app path property value
            ReflectionHelper.SetPrivateInstanceFieldValue("_appDomainAppPath", runtime, PhysicalApplicationPath);
            // set app virtual path property value
            string vpathTypeName = "System.Web.VirtualPath, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            object virtualPath = ReflectionHelper.Instantiate(vpathTypeName, new Type[] { typeof(string) }, new object[] { ApplicationPath });
            ReflectionHelper.SetPrivateInstanceFieldValue("_appDomainAppVPath", runtime, virtualPath);

            // set codegen dir property value
            ReflectionHelper.SetPrivateInstanceFieldValue("_codegenDir", runtime, PhysicalApplicationPath);

            HostingEnvironment environment = GetHostingEnvironment();
            ReflectionHelper.SetPrivateInstanceFieldValue("_appPhysicalPath", environment, PhysicalApplicationPath);
            ReflectionHelper.SetPrivateInstanceFieldValue("_appVirtualPath", environment, virtualPath);
            ReflectionHelper.SetPrivateInstanceFieldValue("_configMapPath", environment, new ConfigMapPath(this));
        }

        protected static HostingEnvironment GetHostingEnvironment()
        {
            HostingEnvironment environment;
            try
            {
                environment = new HostingEnvironment();
            }
            catch (InvalidOperationException)
            {
                //Shoot, we need to grab it via reflection.
                environment = ReflectionHelper.GetStaticFieldValue<HostingEnvironment>("_theHostingEnvironment", typeof(HostingEnvironment));
            }
            return environment;
        }

        #region --- Text Manipulation Methods for slashes ---

        protected static string NormalizeSlashes(string s)
        {
            if (String.IsNullOrEmpty(s) || s == "/")
                return "/";

            s = s.Replace(@"\", "/");

            //Reduce multiple slashes in row to single.
            string normalized = Regex.Replace(s, "(/)/+", "$1");
            //Strip left.
            normalized = StripPrecedingSlashes(normalized);
            //Strip right.
            normalized = StripTrailingSlashes(normalized);
            return "/" + normalized;
        }

        protected static string StripPrecedingSlashes(string s)
        {
            return Regex.Replace(s, "^/*(.*)", "$1");
        }

        protected static string StripTrailingSlashes(string s)
        {
            return Regex.Replace(s, "(.*)/*$", "$1", RegexOptions.RightToLeft);
        }

        protected static string StripTrailingBackSlashes(string s)
        {
            if (String.IsNullOrEmpty(s))
                return string.Empty;
            return Regex.Replace(s, @"(.*)\\*$", "$1", RegexOptions.RightToLeft);
        }

        #endregion --- Text Manipulation Methods for slashes ---

        public class ConfigMapPath : IConfigMapPath
        {
            private HttpSimulator _requestSimulation;

            public ConfigMapPath(HttpSimulator simulation)
            {
                _requestSimulation = simulation;
            }

            public string GetMachineConfigFilename()
            {
                throw new NotImplementedException();
            }

            public string GetRootWebConfigFilename()
            {
                throw new NotImplementedException();
            }

            public void GetPathConfigFilename(string siteID, string path, out string directory, out string baseName)
            {
                throw new NotImplementedException();
            }

            public void GetDefaultSiteNameAndID(out string siteName, out string siteID)
            {
                throw new NotImplementedException();
            }

            public void ResolveSiteArgument(string siteArgument, out string siteName, out string siteID)
            {
                throw new NotImplementedException();
            }

            public string MapPath(string siteID, string path)
            {
                string page = StripPrecedingSlashes(RightAfter(path, _requestSimulation.ApplicationPath));
                return Path.Combine(_requestSimulation.PhysicalApplicationPath, page.Replace("/", @"\"));
            }

            public string GetAppPathForPath(string siteID, string path)
            {
                return _requestSimulation.ApplicationPath;
            }
        }

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current = null;
            }
        }
    }
}