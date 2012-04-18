//
//  Session.cs
//
//  Author:
//       brian <${AuthorEmail}>
//
//  Copyright (c) 2012 brian
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace LacunaExpanse
{
	  
  [Serializable]
  public class Colony
  {
    public Colony() { }  // Default constructor is required for xml.
    
    public Colony(Colony i)
    {    
    }
    
    public string name;
    public string id;
  }
	
	[Serializable]
	public class Cache
	{
    [XmlElement]
		public string password;
		
		public string empire_name;

		public string home_planet_id;

		public string session_id;

		public string time;
	  		        
    [XmlElement(typeof(Colony))]
    public ArrayList colony;
        
    public Cache()
    {
      colony = new ArrayList();
    }
    
    public void Add(Colony i)
    {
      colony.Add(i);
    }
	}

	public class ServerErrorEventArgs : EventArgs
	{
		private int code;
		private string data;
		private string msg;

		public ServerErrorEventArgs() : base()
		{
		}

		public ServerErrorEventArgs(int code, string data, string msg) : base()
		{
			this.code = code;
			this.data = data;
			this.msg = msg;
		}

		public int Code
		{
			get
			{
				return code;
			}
		}

		public string Data
		{
			get
			{
				return data;
			}
		}

		public string Message
		{
			get
			{
				return msg;
			}
		}
	}

	public class ServerStatusEventArgs : EventArgs
	{
		public ServerStatusEventArgs() : base()
		{
		}
	}

	public class Session
	{
		public Cache cache;

		private string serverURL;
		private string sessionLog;
		private string sessionCache;
		private string sessionName;
		
		private NameValueCollection myBucket;
		
		private Dictionary<string, string> myBody;
		private Dictionary<string, string> myEmpire;
		private Dictionary<string, string> myError;
		private Dictionary<string, string> myPlanets;
		private Dictionary<string, string> myResult;
		private Dictionary<string, string> myServer;

		public delegate void CacheData(string result, string member, string data);

		public event CacheData onData;
		
		public delegate void Message(object sender, string format, params object [] arg);
		
		public event Message onMessage;

		public delegate void ServerError(object sender, ServerErrorEventArgs e);

		public event ServerError onServerError;

		public delegate void ServerStatus(object sender, ServerStatusEventArgs e);

		public event ServerStatus onServerStatus;

		static string API_KEY = "anonymous";
		
		private StreamWriter Log;
		
		public static void Main(string[] args)
    {
			Session session = new Session("Empire Server","https://us1.lacunaexpanse.com");

			Console.WriteLine(DateTime.Now.ToString("dd MM yyyy HH:mm:ss zzzz"));
			Console.WriteLine(session.cache.time); // 13 02 2012 14:55:58 +0000

			DateTime date;

			DateTime.TryParseExact(session.cache.time,
			                       "dd MM yyyy HH:mm:ss zzzz",
			                       CultureInfo.InvariantCulture,
			                       DateTimeStyles.AssumeLocal,out date);

			Console.WriteLine(date.ToString("dd MM yyyy HH:mm:ss zzzz"));

			if (session.cache.empire_name == "")
			  session.cache.empire_name = "Your empire name";
			
			if (session.cache.password == "")
			  session.cache.password = "Your password";
			
			// If we already have an active session on server, don't bother with login
			if ((DateTime.Now-date).TotalHours > 2)
			  session.Login(session.cache.empire_name,session.cache.password);
			
			Archaeology.GetGlyphs(session,"1139384");
			Archaeology.BuildHalls(session,"1139384");
			
			/*
			JsonTextWriter req = session.Request("push_items",
			                                     session.cache.session_id,
			                                     "850201","533718");
			req.WriteStartArray();
			session.AddHashedParameters(req, "type","ship","ship_id","144452");
			session.AddHashedParameters(req, "type","ship","ship_id","144453");
			req.WriteEndArray();
			session.AddHashedParameters(req, "ship_id","18904391"); // "stay","1" option
			
			session.Post("/trade",req);
			
			/*
			JsonTextWriter req = session.Request("view_planet",
			                                     session.cache.session_id,
			                                     "1954588","528037");
			
			session.Post("/templeofthedrajilites",req);
    			
			JsonTextWriter req = session.Request("push_items",
			                                     session.cache.session_id,
			                                     "1056038",session.cache.home_planet_id);
			
			req.WriteStartArray ();
			session.AddHashedParameters(req, "type","glyph","glyph_id","6313438");
			req.WriteEndArray ();
			session.AddHashedParameters(req, "ship_id","1798689");
			
			session.Post("/trade",req);
			
			/*
			JsonTextWriter req = session.Request("search_for_glyph",
			                                     session.cache.session_id,
			                                     "1139384","gold");
			session.Post("/archaeology",req);

			/*
			JsonTextWriter req = session.Request("get_buildings",
			                                     session.cache.session_id,
			                                     session.cache.home_planet_id);
			session.Post("/body",req);
			
			/*
			JsonTextWriter req = session.Request("get_stars", //_all_ships",
			                       								session.cache.session_id,
			                       								"-60","220","-90","250");
			session.Post("/map",req);			
			
			/*
			JsonTextWriter req = session.Request("list_planets",
			                                     session.cache.session_id,
			                                     "1954588","73507");
			session.Post("/templeofthedrajilites",req);	
			
			/*
			JsonTextWriter req = session.Request("view_all_ships",
			                                     session.cache.session_id,
			                                     "82335");
			session.Post("/spaceport",req);
			
			/*
			JsonTextWriter req = session.Request("get_probed_stars",
			                                     session.cache.session_id,
			                                     "896833","0");
			session.Post("/observatory",req);			
			
			/*
			JsonTextWriter req = session.Request("rearrange_buildings", //_all_ships",
			                                     session.cache.session_id,
			                                     "523697");
			session.NewArray(req);
			session.AddHashedParameters(req, "id","2128626","x","-4","y","0");

			session.Post("/body",req);
			
			/*
			foreach (KeyValuePair<string,string> planet in session.myPlanets)
			{
				if (planet.Key == session.cache.home_planet_id) continue;
				
				req = session.Request("get_buildings", //_all_ships",
				                       session.cache.session_id,
				                       planet.Key);
				session.Post("/body",req);					
			}

			Client.Request req = new Client.Request("rearrange_buildings",Client.SessionID,"525124");
			
			req.NewArray ();
			req.AddHashedParameters("id","1539522","x","5","y","2");
			req.AddHashedParameters("id","1539395","x","3","y","2");
			*/
		
			session.Close();
			session.Save();
		}

		public Session (string session, string server)
		{
			this.sessionName = session;
			this.serverURL = server;
			
			switch (server)
			{
				case "https://us1.lacunaexpanse.com":
					API_KEY = "6093dbf6-e0cf-4df1-b9ba-707a52120c37";
					break;
				case "https://pt.lacunaexpanse.com":
					API_KEY = "8f5159ad-4e1a-4460-bccf-22e6d56820bc";
					break;
			}

			this.sessionLog = sessionName + ".log";
			this.sessionCache = sessionName + ".xml";
			this.Load();

			if (Regex.IsMatch(sessionLog,sessionName))
				Log = File.CreateText(sessionLog);
			else
				Log = File.AppendText(sessionLog);
			
			myBucket = new NameValueCollection();
			
			myBody = new Dictionary<string, string>();
			myEmpire = new Dictionary<string, string>();
			myError = new Dictionary<string, string>();
			myPlanets = new Dictionary<string, string>();
			myServer = new Dictionary<string, string>();
			myResult = new Dictionary<string, string>();

			//this.onData += UpdateCache;
			this.onMessage += ConsoleMessage;
			this.onServerError += PrintError;
			this.onServerStatus += Status;
		}
		
		public void ConsoleMessage (object sender, string format, params object [] arg)
		{
			Console.WriteLine(format,arg);
		}
		
		public void PrintError (object sender, ServerErrorEventArgs e)
		{
			Console.WriteLine("\nInternal Server Error!");
			Console.WriteLine("Code: {0}",e.Code);
			Console.WriteLine("Data: {0}",e.Data);
			Console.WriteLine(e.Message);
		} 
		
		public void Login (string empire, string password)
		{
			JsonTextWriter req = Request("login",empire,password,API_KEY);

			Post("/empire",req);
		}

		public JsonTextWriter Request (string method, params string [] list)
		{
			JsonTextWriter writer = new JsonTextWriter();

			writer.WriteStartObject ();
			writer.WriteMember ("jsonrpc");
			writer.WriteString ("2.0");
			writer.WriteMember ("id");
			writer.WriteString ("1");
			writer.WriteMember ("method");
			writer.WriteString (method);
			writer.WriteMember ("params");
			writer.WriteStartArray ();

			foreach (string param in list)
				writer.WriteString (param);

			return writer;
		}

		public void AddHashedParameters (JsonTextWriter writer, params string [] list)
		{
			if ( list.Length % 2 > 0) return;

			writer.WriteStartObject ();

			for (int i = 0; i < list.Length; i+=2)
			{
				writer.WriteMember (list[i]);
				writer.WriteString (list[i+1]);
			}

			writer.WriteEndObject ();
		}
		
		public void Close()
		{
			Log.Close();			
		}

	  void Load()
	  {
      if (File.Exists(sessionCache)) {
				XmlSerializer serializer = new XmlSerializer(typeof(Cache));
	      Stream stream = new FileStream(sessionCache,FileMode.Open);
	      cache = (Cache) serializer.Deserialize(stream);
	      stream.Close();
			} else {
				cache = new Cache();
			}
    }

		// Return error code
		public int Post (string url, JsonTextWriter json)
		{
      HttpWebRequest request = (HttpWebRequest) WebRequest.Create (serverURL+url);

      request.Method = "POST";

			json.AutoComplete();

			string text = json.ToString();

    	Log.Write("Request: ");
    	Log.WriteLine (text.ToString()+"\n");

			json.Close();

      byte[] postdata = System.Text.Encoding.ASCII.GetBytes (text);

			request.ContentLength = postdata.Length;

			Stream stream = request.GetRequestStream ();

      stream.Write (postdata, 0, postdata.Length);
      stream.Close ();

			//Log = File.AppendText("TLE.log");

			if (onMessage != null) onMessage(this,"\n[{0}] Connecting...",DateTime.Now);

			//Log.WriteLine("[{0}] Request sent",DateTime.UtcNow);
			//Log.WriteLine("{0} {1}",URL,text);

			try {
      	WebResponse response = request.GetResponse ();
				ProcessResponse(response);
				if (onServerStatus != null) onServerStatus(this,new ServerStatusEventArgs());
			} catch (System.Net.WebException ex) {
				if (Regex.IsMatch(ex.Response.ContentType,"json-rpc")) {
				  WebResponse response = ex.Response;
					ProcessResponse(response);
					ServerErrorEventArgs args =
						new ServerErrorEventArgs(int.Parse(myError["code"]),
						                         myError["data"],myError["message"]);
					if (onServerError != null) onServerError(this,args);
					return int.Parse(myError["code"]);
				} else {
					if (onMessage != null) onMessage(this,ex.Message);
					return -1;
				}
			}			
			return 0;
		}

		public void ProcessResponse(WebResponse response)
		{
			StreamReader sr = new StreamReader(response.GetResponseStream());

			if (onMessage != null) onMessage(this,"[{0}] Reading response...",DateTime.Now);

			string data = sr.ReadToEnd ();

			response.Close ();

    	Log.Write("Response: ");
    	Log.WriteLine (data.ToString()+"\n");

			if (onMessage != null) onMessage(this,"[{0}] Processing response...",DateTime.Now);

			using (JsonTextReader reader = new JsonTextReader(new StringReader(data)))
			{
				string result="",text = "";
				
				Stack<string> level = new Stack<string>();
				Stack<string> token = new Stack<string>();
				
				while(reader.Read())
				{
					switch (reader.TokenClass.Name)
					{
						case "Member":
							Log.Write("[{0}] {1} : ",level.Count,text = reader.Text);
							break;
						case "Array":
						case "Object":
							if (text == "") 
								Log.WriteLine("[{0}] {1} ()",level.Count,reader.TokenClass);
							else
								Log.WriteLine("({0})",reader.TokenClass);
							if (level.Count < 3) result = text;
							level.Push(text);
							text = "";
							break;
						case "EndArray":
						case "EndObject":
							Log.WriteLine("[{0}] {1} ({2})",level.Count-1,reader.TokenClass,level.Pop());
							if (level.Count < 3 && level.Count > 0 ) 
								result = level.Peek();
							break;
						case "BOF":
						case "EOF":
							break;
						default :
							Log.WriteLine("{0} ({1})",reader.Text,reader.TokenClass);
							switch (result)
							{
								case "error":
									if (myError.ContainsKey(text))
										myError[text] = reader.Text;
									else
										myError.Add(text,reader.Text);
									break;
								case "result":
								case "status":
									UpdateStatus(level.Peek(),text,reader.Text);
									break;
								default:
									if (onData != null) onData(result,text,reader.Text);
									text = "";
									break;									
							}
							break;
					}
				}
				
				if (onMessage != null) onMessage(this,"[{0}] Processing complete!\n",DateTime.Now);

				Log.WriteLine("-------------------------");
			}
		}

	  void Save()
	  {
	    XmlSerializer serializer = new XmlSerializer(typeof(Cache));
	    Stream stream = new FileStream(sessionCache,FileMode.Create);
      serializer.Serialize(stream,cache);
      stream.Close();
	  }

		void Status (object sender, ServerStatusEventArgs e)
		{
			if (onMessage != null) onMessage(this,"Success!\n");
		}

		void UpdateStatus(string result, string member, string text)
		{
			switch (result)
			{
				case "body":
					if (myBody.ContainsKey(member))
						myBody[member] = text;
					else
						myBody.Add(member,text);
					break;
				case "empire":
					if (myEmpire.ContainsKey(member))
						myEmpire[member] = text;
					else
						myEmpire.Add(member,text);
					switch (member)
					{
						case "home_planet_id":
							cache.home_planet_id = text;
							break;
					}
					break;/*
				case "error":
					if (myError.ContainsKey(member))
						myError[member] = text;
					else
						myError.Add(member,text);
					break;*/
				case "planets":
					if (myPlanets.ContainsKey(member)) break;
					myPlanets.Add(member,text);
					break;
				case "result":
					if (myResult.ContainsKey(member))
						myResult[member] = text;
					else
						myResult.Add(member,text);
					switch (member)
					{
						case "session_id":
							cache.session_id = text;
							break;
					}
					break;
				case "server":
					if (myServer.ContainsKey(member))
						myServer[member] = text;
					else
						myServer.Add(member,text);
					switch (member)
					{
						case "time":
							cache.time = text;
							break;
					}
					break;
				default:
					myBucket.Add(member,text);
					break;
			}
		}
	}
}