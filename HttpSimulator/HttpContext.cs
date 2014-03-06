using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Http.TestLibrary
{
    class HttpContext2:HttpContextBase
    {
        private readonly HttpRequestBase _workerRequest;
        private readonly HttpSessionStateBase _fakeHttpSessionState;
        private HttpApplicationStateBase _application;

        public HttpContext2(HttpRequestBase workerRequest, HttpSessionStateBase fakeHttpSessionState)
        {
            _workerRequest = workerRequest;
            _fakeHttpSessionState = fakeHttpSessionState;
        }

        public override HttpApplicationStateBase Application
        {
            get { return _application; }
        }

        public override HttpSessionStateBase Session
        {
            get { return _fakeHttpSessionState; }
        }

        public override HttpRequestBase Request
        {
            get { return _workerRequest; }
        }
    }
}
