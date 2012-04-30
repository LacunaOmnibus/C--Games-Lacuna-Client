// 
//  Result.cs
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
using System.Collections;
using System.Collections.Generic;

namespace LacunaExpanse
{
  public class Response
  {
    public int id;
    public string jsonrpc;

    public class Error
    {
      public int code;
      public string data;
      public string message;
    }
    public Error error;

    public class Result
    {
      public class Body
      {
        public string surface_image;
      }
      public Body body;
      public Dictionary<string,string> boosts;

      public class Building
      {
        public string id; // Internal use only, not part of response
        public string y;
        public string efficiency;
        public string level;
        public string name;
        public string url;
        public string x;
        public string image;
  
        public class Work
        {
          public int seconds_remaining;
          public string end;
          public string start;
        }
        public Work work;
      }
      public Dictionary<string,Building> buildings;

      public class EffectBHG
      {
        public Dictionary<string,string> fail;
        public Dictionary<string,string> side;
        public Dictionary<string,string> target;
      }
      public EffectBHG effect;
  
      public class Glyph
      {
        public string id;
        public string type;
      }
      public Glyph [] glyphs;
      public object map;
  
      public class Plan
      {
        public string id;
        public string name;
        public string level;
        public string extra_build_level;
      }
      public Plan [] plans;
  
      public class Prisoner
      {
        public string id;
        public string name;
        public string level;
      }
      public Prisoner [] prisoners;
  
      public object reserve;
      public Dictionary<string,int> resources;
  
      public class Ship
      {
        public int can_recall;
        public string fleet_speed;
        public string name;
        public string date_available;
        public string task;
        public int max_occupants;
        public string combat;
        public string stealth;
        public int can_scuttle;
        public string speed;
        public string berth_level;
        public string hold_size;
        public string id;
        public string type;
        public string type_human;
        public string date_started;
        public string estimated_travel_time;
  
        //public Payload [] payload;
      }
      public Ship [] ships;
  
      public class Status
      {
        public class Body
        {
          public Dictionary<string,int> ore;
          public string name;
        }
        public Body body;
  
        public class Empire
        {
          public Dictionary<string,string> planets;
          public string home_planet_id;
          public int rpc_count;
        }
        public Empire empire;
  
        public class Server
        {
          public class MapSize
          {
            public int [] x,y;
          }
          public MapSize star_map_size;
          public int rpc_limit;
          public string time;
          public string version;
        }
        public Server server;
      }
      public Status status;
  
      public string cargo_space_used_each;
      public string number_of_ships;
      public string session_id;
    }
    public Result result;
  }
}

