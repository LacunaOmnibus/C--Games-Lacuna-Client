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
using System.Threading;
using System.Timers;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace LacunaExpanse
{
	/*
	[Food] 	+25% food per hour 	5 		 
	[Ore] 	+25% ore per hour 	5 		 
	[Water] 	+25% water per hour 	5 		 
	[Energy] 	+25% energy per hour 	5 		 
	[Happiness] 	+25% happiness per hour 	5 		 
	[Storage] 	+25% storage capacity 	5 		9:53:28
	[Building] 	+25% build speed 	5 		
	*/
	
  [Serializable]
  public class Colony
  {
    public Colony() { }  // Default constructor is required for xml.
    
    public Colony(Colony c)
    {    
    }
		
		public Colony (string cKey, string cName)
		{
			id = cKey;
			name = cName;
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
    public string server;
    public string database;

    public string session_id;

    public string time;
    
    public int rpcCount;
    public int rpcLimit;
		
		public string boostEnergy;
		public string boostOre;
		public string boostFood;
		public string boostBuilding;
		public string boostStorage;
		public string boostWater;
		public string boostHappiness;

    public string home_planet_id;
		
    [XmlElement(typeof(Colony))]
    public ArrayList colony;
        
    public Cache()
    {
      colony = new ArrayList();
    }
    
    public void Add(Colony c)
    {
      colony.Add(c);
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

    private Response r;

		private string serverURL;
		private string sessionLog;
		private string sessionCache;
		private string sessionName;

		private Dictionary<string, Colony> myColony;
		
		public delegate void Message(object sender, string format, params object [] arg);
		
		public event Message onMessage;

		public delegate void ServerError(object sender, ServerErrorEventArgs e);

		public event ServerError onServerError;

		public delegate void ServerStatus(object sender, ServerStatusEventArgs e);

		public event ServerStatus onServerStatus;

		static string API_KEY = "anonymous";
		
		private StreamWriter Log;
	
		/*
		0 - Emergency (emerg)
		1 - Alerts (alert)
		2 - Critical (crit)
		3 - Errors (err)
		4 - Warnings (warn)
		5 - Notification (notice)
		6 - Information (info)
		7 - Debug (debug) 	
		*/
		
		static int severity = 6;
		
		static DateTime start;
		
		static long throttle = 1;
		
		static System.Timers.Timer timer;
		
		public static void Main(string[] args)
    {
			Session session = new Session("Empire Server","https://us1.lacunaexpanse.com");
      
      //Session session = new Session("Empire Server",null);

			Console.WriteLine(DateTime.Now.ToString("dd MM yyyy HH:mm:ss zzzz"));
			Console.WriteLine(session.cache.time); // 13 02 2012 14:55:58 +0000

			DateTime date;

			DateTime.TryParseExact(session.cache.time,
			                       "dd MM yyyy HH:mm:ss zzzz",
			                       CultureInfo.InvariantCulture,
			                       DateTimeStyles.AssumeLocal,out date);

			Console.WriteLine(date.ToString("dd MM yyyy HH:mm:ss zzzz"));

			if (session.cache.empire_name == null)
			  session.cache.empire_name = "name";
			
			if (session.cache.password == null)
			  session.cache.password = "password";
			
			timer = new System.Timers.Timer(1000); // Setup timer with one second interval
			timer.Elapsed += TimerTick;
			timer.Start();
			
			start = DateTime.Now;

      //DirectoryInfo dir = new DirectoryInfo("." + Path.DirectorySeparatorChar);

      //System.IO.FileInfo [] info = dir.GetFiles("*.xml");

      //foreach (FileInfo f in info) Console.WriteLine(f.Name);
			
			// If we already have an active session on server, don't bother with login
			if ((DateTime.Now-date).TotalHours > 2)
        session.Login(session.cache.empire_name, session.cache.password);

      //foreach (int i in Sqlite.GetNearestStars (208,250,30))
        //Console.WriteLine (i);
	
			//Archaeology.GetGlyphs(session,"88311"); //Dot Net 3
			//Archaeology.BuildHalls(session,"88311",5);
			
			//Archaeology.GetGlyphs(session,"1139384"); // Nexus Bend
			//Archaeology.BuildHalls(session,"1139384",0);
			

      Body.GetBuildings(session,session.cache.home_planet_id);

      /*
      JsonTextWriter req = session.Request("rearrange_buildings",
                                           session.cache.session_id,
                                           "530864");
      req.WriteStartArray();
      session.AddHashedParameters(req, "id","815960","x","-5","y","-1");
      session.AddHashedParameters(req, "id","822541","x","3","y","0");
      session.AddHashedParameters(req, "id","2001714","x","2","y","-3");

      session.Post("/body",req);
      /*
			JsonTextWriter req = session.Request("generate_singularity",
			                                     session.cache.session_id,
			                                     "2604813");
			                                     
			session.AddHashedParameters(req,"body_name","Reaver Empire");
			req.WriteString("Increase Size");

			session.Post("/blackholegenerator",req);

      Body.GetBuildings(session,"523697");

      foreach (string key in session.r.result.buildings.Keys)
      {
        if (Regex.IsMatch(session.r.result.buildings[key].name,"Port"))
        {
          JsonTextWriter req = session.Request("view_all_ships",
                                               session.cache.session_id,
                                               key);

          session.AddHashedParameters(req,"no_paging","1");
          session.Post("/spaceport",req);
          break;
        }
      }

			JsonTextWriter req = session.Request("generate_singularity",
			                                     session.cache.session_id,
			                                     "80348");
			session.AddHashedParameters(req,"body_name","DeLambert-10-341");
			req.WriteString("Make Asteroid");
			
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
			

			JsonTextWriter req = session.Request("search_for_glyph",
			                                     session.cache.session_id,
			                                     "1139384","gold");
			session.Post("/archaeology",req);


			JsonTextWriter req = session.Request("get_buildings",
			                                     session.cache.session_id,
			                                     session.cache.home_planet_id);
			session.Post("/body",req);
			

			JsonTextWriter req = session.Request("get_stars", //_all_ships",
			                       								session.cache.session_id,
			                       								"-60","220","-90","250");
			session.Post("/map",req);			
			

			JsonTextWriter req = session.Request("list_planets",
			                                     session.cache.session_id,
			                                     "2762601","73507");
			session.Post("/templeofthedrajilites",req);	
			

      JsonTextWriter req = session.Request("view_all_ships",
                                           session.cache.session_id,
                                           "2572252");

      session.AddHashedParameters(req,"no_paging","1");
      session.Post("/spaceport",req);
			

			JsonTextWriter req = session.Request("get_probed_stars",
			                                     session.cache.session_id,
			                                     "896833","0");
			session.Post("/observatory",req);

			JsonTextWriter req = session.Request("rearrange_buildings", //_all_ships",
			                                     session.cache.session_id,
			                                     "523697");
			session.NewArray(req);
			session.AddHashedParameters(req, "id","1056035","x","-4","y","0");

			session.Post("/body",req);
			

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
		
		static void TimerTick (object sender, System.Timers.ElapsedEventArgs e)
		{
			if ((DateTime.Now-start).TotalSeconds > 60 && Interlocked.Read(ref throttle) > 0)
				Interlocked.Decrement(ref throttle);
		}

		public Session (string session, string server)
		{
			sessionName = session;

      sessionLog = sessionName + ".log";
      sessionCache = sessionName + ".xml";
      Load();

      if (server != null) cache.server = server;

      serverURL = cache.server;
			
			switch (serverURL)
			{
				case "https://us1.lacunaexpanse.com":
					API_KEY = "6093dbf6-e0cf-4df1-b9ba-707a52120c37";
          //Sqlite.dbName = "us1.db";
					break;
				case "https://pt.lacunaexpanse.com":
					API_KEY = "8f5159ad-4e1a-4460-bccf-22e6d56820bc";
          //Sqlite.dbName = "pt.db";
					break;
			}
			
			if (Regex.IsMatch(sessionLog,sessionName))
				Log = File.CreateText(sessionLog);
			else
				Log = File.AppendText(sessionLog);

			myColony = new Dictionary<string, Colony>();
			
			foreach (Colony c in cache.colony)
				myColony.Add(c.id,c);
			
			//onData += UpdateCache;
			onMessage += ConsoleMessage;
			onServerError += PrintError;
			onServerStatus += Status;
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
			
			if (Post("/empire",req) == 0) cache.session_id = r.result.session_id;
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
      WebRequest request = WebRequest.Create (serverURL+url);

      request.Method = "POST";

			json.AutoComplete();

			string text = json.ToString();

    	Log.Write("Request: ");
    	Log.WriteLine (text.ToString()+"\n");

			json.Close();

      byte[] postdata = System.Text.Encoding.ASCII.GetBytes (text);

			request.ContentLength = postdata.Length;
			
			// Wait one second when 60th RPC call is made during minute
			if (Interlocked.Read(ref throttle) == 60)
			{
				if (onMessage != null && severity >= 6) 
					onMessage(this,"[{0}] Throttling...", DateTime.Now);
				while (Interlocked.Read(ref throttle) == 60)
					Thread.Sleep(1000);
			}
			
			if (timer.Enabled) Interlocked.Increment(ref throttle);
      
			try {
				Stream stream = request.GetRequestStream ();

      	stream.Write (postdata, 0, postdata.Length);
      	stream.Close ();
			} catch (System.Net.WebException ex) {
				if (onMessage != null && severity >= 3) onMessage(this,ex.Message);
				return -1;				
			}

			//Log = File.AppendText("TLE.log");

			if (onMessage != null && severity >= 7)
				onMessage(this,"\n[{0}] Connecting...",DateTime.Now);

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
						new ServerErrorEventArgs(r.error.code,r.error.data,r.error.message);
					if (onServerError != null) onServerError(this,args);
					return r.error.code;
				} else {
					if (onMessage != null && severity >= 3) onMessage(this,ex.Message);
					return -1;
				}
			}			
			return 0;
		}

		public void ProcessResponse(WebResponse response)
		{
			StreamReader sr = new StreamReader(response.GetResponseStream());

			if (onMessage != null && severity >= 7)
				onMessage(this,"[{0}] Reading response...",DateTime.Now);

			string data = sr.ReadToEnd ();

			response.Close ();

    	Log.Write("Response: ");
    	Log.WriteLine (data.ToString()+"\n");

			if (onMessage != null && severity >= 7)
				onMessage(this,"[{0}] Processing response...",DateTime.Now);

			using (JsonTextReader reader = new JsonTextReader(new StringReader(data)))
			{
        string text = "";
				
				Stack<string> level = new Stack<string>();
				//Stack<string> token = new Stack<string>();
				
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
							level.Push(text);
              text = "";
							break;
						case "EndArray":
						case "EndObject":
							Log.WriteLine("[{0}] {1} ({2})",level.Count-1,reader.TokenClass,level.Pop());
							break;
						case "BOF":
						case "EOF":
							break;
						default :
							Log.WriteLine("{0} ({1})",reader.Text,reader.TokenClass);
							break;
					}
				}
				
				if (onMessage != null && severity >= 7)
					onMessage(this,"[{0}] Processing complete!\n",DateTime.Now);
				
				if (onMessage != null && severity >= 6)
					onMessage(this,"\n[{0}] {1} RPC call(s) remaining today",
					          DateTime.Now, cache.rpcLimit - cache.rpcCount);

				Log.WriteLine("-------------------------");
        Log.Flush();  // if we crash when we Deserialize, we still get the log!
			}

      JavaScriptSerializer js = new JavaScriptSerializer();

      r = js.Deserialize<Response>(data);

      if (r.error != null) return;

      // Cache what we need from status, if no errors
      cache.time = r.result.status.server.time;
      cache.rpcCount = r.result.status.empire.rpc_count;
      cache.rpcLimit = r.result.status.server.rpc_limit;
      cache.home_planet_id = r.result.status.empire.home_planet_id;

      // Update or add planet names to session cache
      foreach (KeyValuePair<string,string> p in r.result.status.empire.planets)
      {
        if (myColony.ContainsKey(p.Key)) {
          myColony[p.Key].name = p.Value;
        } else {
          Colony colony = new Colony(p.Key,p.Value);
          myColony.Add(p.Key,colony);
          cache.colony.Add(colony);
        }
      }
		}

    public Response response
    {
      get { return r; }
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
			if (onMessage != null && severity >= 7) onMessage(this,"Success!\n");
		}
	}
}