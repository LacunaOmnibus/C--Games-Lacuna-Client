// 
//  Infrastructure.cs
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
using System.Xml.Serialization;

namespace LacunaExpanse
{
  [Serializable]
  public class Infrastructure
  {
    [XmlElement(typeof(Response.Result.Building))]
    public ArrayList building;

    public string date;

    public Infrastructure()
    {
      building = new ArrayList();
    }

    public void Add(Response.Result.Building b)
    {
      building.Add(b);
    }
  }
}

