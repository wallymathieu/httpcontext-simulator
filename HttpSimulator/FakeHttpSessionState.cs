using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;

namespace Http.TestLibrary
{
    public class FakeHttpSessionState : NameObjectCollectionBase, IHttpSessionState
    {
        private string sessionID = Guid.NewGuid().ToString();
        private int timeout = 30; //minutes
        private bool isNewSession = true;
        private int lcid;
        private int codePage;
        private HttpStaticObjectsCollection staticObjects = new HttpStaticObjectsCollection();
        private object syncRoot = new Object();

        ///<summary>
        ///Ends the current session.
        ///</summary>
        ///
        public void Abandon()
        {
            BaseClear();
        }

        ///<summary>
        ///Adds a new item to the session-state collection.
        ///</summary>
        ///
        ///<param name="name">The name of the item to add to the session-state collection. </param>
        ///<param name="value">The value of the item to add to the session-state collection. </param>
        public void Add(string name, object value)
        {
            BaseAdd(name, value);
        }

        ///<summary>
        ///Deletes an item from the session-state item collection.
        ///</summary>
        ///
        ///<param name="name">The name of the item to delete from the session-state item collection. </param>
        public void Remove(string name)
        {
            BaseRemove(name);
        }

        ///<summary>
        ///Deletes an item at a specified index from the session-state item collection.
        ///</summary>
        ///
        ///<param name="index">The index of the item to remove from the session-state collection. </param>
        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        ///<summary>
        ///Clears all values from the session-state item collection.
        ///</summary>
        ///
        public void Clear()
        {
            BaseClear();
        }

        ///<summary>
        ///Clears all values from the session-state item collection.
        ///</summary>
        ///
        public void RemoveAll()
        {
            BaseClear();
        }

        ///<summary>
        ///Copies the collection of session-state item values to a one-dimensional array, starting at the specified index in the array.
        ///</summary>
        ///
        ///<param name="array">The <see cref="T:System.Array"></see> that receives the session values. </param>
        ///<param name="index">The index in array where copying starts. </param>
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///Gets the unique session identifier for the session.
        ///</summary>
        ///
        ///<returns>
        ///The session ID.
        ///</returns>
        ///
        public string SessionID
        {
            get { return sessionID; }
        }

        ///<summary>
        ///Gets and sets the time-out period (in minutes) allowed between requests before the session-state provider terminates the session.
        ///</summary>
        ///
        ///<returns>
        ///The time-out period, in minutes.
        ///</returns>
        ///
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        ///<summary>
        ///Gets a value indicating whether the session was created with the current request.
        ///</summary>
        ///
        ///<returns>
        ///true if the session was created with the current request; otherwise, false.
        ///</returns>
        ///
        public bool IsNewSession
        {
            get { return isNewSession; }
        }

        ///<summary>
        ///Gets the current session-state mode.
        ///</summary>
        ///
        ///<returns>
        ///One of the <see cref="T:System.Web.SessionState.SessionStateMode"></see> values.
        ///</returns>
        ///
        public SessionStateMode Mode
        {
            get { return SessionStateMode.InProc; }
        }

        ///<summary>
        ///Gets a value indicating whether the session ID is embedded in the URL or stored in an HTTP cookie.
        ///</summary>
        ///
        ///<returns>
        ///true if the session is embedded in the URL; otherwise, false.
        ///</returns>
        ///
        public bool IsCookieless
        {
            get { return false; }
        }

        ///<summary>
        ///Gets a value that indicates whether the application is configured for cookieless sessions.
        ///</summary>
        ///
        ///<returns>
        ///One of the <see cref="T:System.Web.HttpCookieMode"></see> values that indicate whether the application is configured for cookieless sessions. The default is <see cref="F:System.Web.HttpCookieMode.UseCookies"></see>.
        ///</returns>
        ///
        public HttpCookieMode CookieMode
        {
            get { return HttpCookieMode.UseCookies; }
        }

        ///<summary>
        ///Gets or sets the locale identifier (LCID) of the current session.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Globalization.CultureInfo"></see> instance that specifies the culture of the current session.
        ///</returns>
        ///
        public int LCID
        {
            get { return lcid; }
            set { lcid = value; }
        }

        ///<summary>
        ///Gets or sets the code-page identifier for the current session.
        ///</summary>
        ///
        ///<returns>
        ///The code-page identifier for the current session.
        ///</returns>
        ///
        public int CodePage
        {
            get { return codePage; }
            set { codePage = value; }
        }

        ///<summary>
        ///Gets a collection of objects declared by &lt;object Runat="Server" Scope="Session"/&gt; tags within the ASP.NET application file Global.asax.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Web.HttpStaticObjectsCollection"></see> containing objects declared in the Global.asax file.
        ///</returns>
        ///
        public HttpStaticObjectsCollection StaticObjects
        {
            get { return staticObjects; }
        }

        ///<summary>
        ///Gets or sets a session-state item value by name.
        ///</summary>
        ///
        ///<returns>
        ///The session-state item value specified in the name parameter.
        ///</returns>
        ///
        ///<param name="name">The key name of the session-state item value. </param>
        public object this[string name]
        {
            get { return BaseGet(name); }
            set { BaseSet(name, value); }
        }

        ///<summary>
        ///Gets or sets a session-state item value by numerical index.
        ///</summary>
        ///
        ///<returns>
        ///The session-state item value specified in the index parameter.
        ///</returns>
        ///
        ///<param name="index">The numerical index of the session-state item value. </param>
        public object this[int index]
        {
            get { return BaseGet(index); }
            set { BaseSet(index, value); }
        }

        ///<summary>
        ///Gets an object that can be used to synchronize access to the collection of session-state values.
        ///</summary>
        ///
        ///<returns>
        ///An object that can be used to synchronize access to the collection.
        ///</returns>
        ///
        public object SyncRoot
        {
            get { return syncRoot; }
        }

        ///<summary>
        ///Gets a value indicating whether access to the collection of session-state values is synchronized (thread safe).
        ///</summary>
        ///<returns>
        ///true if access to the collection is synchronized (thread safe); otherwise, false.
        ///</returns>
        ///
        public bool IsSynchronized
        {
            get { return true; }
        }

        ///<summary>
        ///Gets a value indicating whether the session is read-only.
        ///</summary>
        ///
        ///<returns>
        ///true if the session is read-only; otherwise, false.
        ///</returns>
        ///
        bool IHttpSessionState.IsReadOnly
        {
            get
            {
                return true;
            }
        }
    }
}