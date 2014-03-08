using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Http.TestLibrary
{
    class HttpContextWrapper:HttpContextBase
    {
        private readonly HttpRequestBase _workerRequest;
        private readonly HttpSessionStateBase _fakeHttpSessionState;

        public HttpContextWrapper(HttpRequestBase workerRequest, HttpSessionStateBase fakeHttpSessionState)
        {
            _workerRequest = workerRequest;
            _fakeHttpSessionState = fakeHttpSessionState;
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
