// 
//  Body.cs
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
using System.Xml;
using System.Xml.Serialization;

using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace LacunaExpanse
{
  public class Body
  {
    static string URL = "/body";

    public static void GetStatus (Session session, string bodyID)
    {
      JsonTextWriter req = session.Request("view",
                                           session.cache.session_id,
                                           bodyID);

      session.Post(URL,req);
    }

    public static void GetBuildings (Session session, string bodyID)
    {
      JsonTextWriter req = session.Request("get_buildings",
                                           session.cache.session_id,
                                           bodyID);

      if (session.Post(URL,req) == 0)
      {
        Response.Result result = session.response.result as Response.Result;

        IEnumerator b = result.buildings.GetEnumerator();

        Infrastructure buildings = new Infrastructure();

        buildings.date = result.status.server.time;  // save server time
  
        while (b.MoveNext())
        {
          KeyValuePair<string,Response.Result.Building> k =
            (KeyValuePair<string,Response.Result.Building>) b.Current;
  
          k.Value.id = k.Key;

          buildings.Add(k.Value);
        }

        string filename = String.Format("{0}.xml",result.status.body.name);

        XmlSerializer serializer = new XmlSerializer(typeof(Infrastructure));
        Stream stream = new FileStream(filename,FileMode.Create);
        serializer.Serialize(stream,buildings);
        stream.Close();
      }
    }

    public static void GetBuildable (Session session, string bodyID, string x, string y, string tag)
    {
      JsonTextWriter req = session.Request("get_buildable",
                                           session.cache.session_id,
                                           bodyID,x,y,tag);

      session.Post(URL,req);
    }

    public static void Rename (Session session, string bodyID, string name)
    {
      JsonTextWriter req = session.Request("rename",
                                           session.cache.session_id,
                                           bodyID,name);

      session.Post(URL,req);
    }

    public static void Abandon (Session session, string bodyID)
    {
      JsonTextWriter req = session.Request("abandon",
                                           session.cache.session_id,
                                           bodyID);

      session.Post(URL,req);
    }
  }
}

