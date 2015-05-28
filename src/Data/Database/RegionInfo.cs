﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text.RegularExpressions;
using Aura.Mabi.Const;
using System.Xml.Linq;

namespace Aura.Data.Database
{
	public class RegionInfoData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int GroupId { get; set; }
		public int X1 { get; set; }
		public int Y1 { get; set; }
		public int X2 { get; set; }
		public int Y2 { get; set; }
		public List<AreaData> Areas { get; set; }

		public RegionInfoData()
		{
			this.Areas = new List<AreaData>();
		}

		/// <summary>
		/// Returns id of area at the given coordinates, or 0 if area wasn't found.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int GetAreaId(int x, int y)
		{
			foreach (var area in this.Areas)
			{
				if (x >= Math.Min(area.X1, area.X2) && x <= Math.Max(area.X1, area.X2) && y >= Math.Min(area.Y1, area.Y2) && y <= Math.Max(area.Y1, area.Y2))
					return area.Id;
			}

			return 0;
		}

		/// <summary>
		/// Returns area with given name or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public AreaData GetArea(string name)
		{
			return this.Areas.FirstOrDefault(a => a.Name == name);
		}

		/// <summary>
		/// Returns event by id or null if it doesn't exist.
		/// </summary>
		/// <returns></returns>
		public EventData GetEvent(long eventId)
		{
			foreach (var area in this.Areas)
			{
				if (area.Events.ContainsKey(eventId))
					return area.Events[eventId];
			}

			return null;
		}

		/// <summary>
		/// Returns index of the area in the list.
		/// </summary>
		/// <param name="areaId"></param>
		/// <returns></returns>
		public int GetAreaIndex(int areaId)
		{
			var id = 1;
			foreach (var area in this.Areas)
			{
				if (area.Id == areaId)
					return id;

				id++;
			}

			return -1;
		}

		/// <summary>
		/// Returns random coordinates inside the actual region.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public Point RandomCoord(Random rnd)
		{
			var result = new Point();
			result.X = rnd.Next(this.X1, this.X2);
			result.Y = rnd.Next(this.Y1, this.Y2);

			return result;
		}

		/// <summary>
		/// Creates copy of this region data.
		/// </summary>
		/// <returns></returns>
		public RegionInfoData Copy()
		{
			var result = new RegionInfoData();
			result.Id = this.Id;
			result.Name = this.Name;
			result.GroupId = this.GroupId;
			result.X1 = this.X1;
			result.Y1 = this.Y1;
			result.X2 = this.X2;
			result.Y2 = this.Y2;

			foreach (var area in this.Areas)
				result.Areas.Add(area.Copy(true, true));

			return result;
		}
	}

	public class AreaData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int X1 { get; set; }
		public int Y1 { get; set; }
		public int X2 { get; set; }
		public int Y2 { get; set; }
		public Dictionary<long, PropData> Props { get; set; }
		public Dictionary<long, EventData> Events { get; set; }

		public AreaData()
		{
			this.Props = new Dictionary<long, PropData>();
			this.Events = new Dictionary<long, EventData>();
		}

		/// <summary>
		/// Creates a copy of the area data.
		/// </summary>
		/// <param name="copyProps"></param>
		/// <param name="copyEvents"></param>
		/// <returns></returns>
		public AreaData Copy(bool copyProps, bool copyEvents)
		{
			var result = new AreaData();
			result.Id = this.Id;
			result.X1 = this.X1;
			result.Y1 = this.Y1;
			result.X2 = this.X2;
			result.Y2 = this.Y2;
			result.Props = new Dictionary<long, PropData>();
			result.Events = new Dictionary<long, EventData>();

			if (copyProps)
			{
				foreach (var original in this.Props.Values)
				{
					var item = original.Copy();
					result.Props.Add(item.EntityId, item);
				}
			}

			if (copyEvents)
			{
				foreach (var original in this.Events.Values)
				{
					var item = original.Copy();
					result.Events.Add(item.Id, item);
				}
			}

			return result;
		}

		/// <summary>
		/// Returns prop with given name or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public PropData GetProp(string name)
		{
			return this.Props.Values.FirstOrDefault(a => a.Name.ToLower() == name.ToLower());
		}

		/// <summary>
		/// Returns event with given name or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public EventData GetEvent(string name)
		{
			return this.Events.Values.FirstOrDefault(a => a.Name.ToLower() == name.ToLower());
		}
	}

	public class PropData
	{
		public long EntityId { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Direction { get; set; }
		public float Scale { get; set; }
		public List<ShapeData> Shapes { get; set; }
		public List<RegionElementData> Parameters { get; set; }

		public PropData()
		{
			this.Shapes = new List<ShapeData>();
			this.Parameters = new List<RegionElementData>();
		}

		/// <summary>
		/// Returns drop type, if one exists, or -1.
		/// </summary>
		/// <returns></returns>
		public int GetDropType()
		{
			foreach (var param in this.Parameters)
			{
				// TODO: Event or SignalType can probably be checked as
				//   well for finding drop props.
				if (param.XML == null || param.XML.Attribute("droptype") == null)
					continue;

				return int.Parse(param.XML.Attribute("droptype").Value);
			}

			return -1;
		}

		public PropData Copy()
		{
			var result = new PropData();
			result.EntityId = this.EntityId;
			result.Id = this.Id;
			result.X = this.X;
			result.Y = this.Y;
			result.Direction = this.Direction;
			result.Scale = this.Scale;

			result.Shapes = new List<ShapeData>(this.Shapes.Count);
			foreach (var item in this.Shapes)
				result.Shapes.Add(item.Copy());

			result.Parameters = new List<RegionElementData>(this.Parameters.Count);
			foreach (var item in this.Parameters)
				result.Parameters.Add(item.Copy());

			return result;
		}
	}

	public class ShapeData
	{
		public int X1 { get; set; }
		public int Y1 { get; set; }
		public int X2 { get; set; }
		public int Y2 { get; set; }
		public int X3 { get; set; }
		public int Y3 { get; set; }
		public int X4 { get; set; }
		public int Y4 { get; set; }

		public ShapeData Copy()
		{
			var result = new ShapeData();
			result.X1 = this.X1;
			result.Y1 = this.Y1;
			result.X2 = this.X2;
			result.Y2 = this.Y2;
			result.X3 = this.X3;
			result.Y3 = this.Y3;
			result.X4 = this.X4;
			result.Y4 = this.Y4;

			return result;
		}
	}

	public class EventData
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public EventType Type { get; set; }
		public int RegionId { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public bool IsAltar { get; set; }
		public List<ShapeData> Shapes { get; set; }
		public List<RegionElementData> Parameters { get; set; }

		public EventData()
		{
			this.Shapes = new List<ShapeData>();
			this.Parameters = new List<RegionElementData>();
		}

		public bool IsInside(int x, int y)
		{
			if (this.Shapes.Count == 0)
				return false;

			var result = false;

			var shape = this.Shapes[0];
			var point = new Point(x, y);
			var points = new[] // >_>
			{
				new Point(shape.X1,shape.Y1),
				new Point(shape.X2,shape.Y2),
				new Point(shape.X3,shape.Y3),
				new Point(shape.X4,shape.Y4),
			};

			for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
			{
				if (((points[i].Y > point.Y) != (points[j].Y > point.Y)) && (point.X < (points[j].X - points[i].X) * (point.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X))
					result = !result;
			}

			return result;
		}

		public EventData Copy()
		{
			var result = new EventData();
			result.Id = this.Id;
			result.Type = this.Type;
			result.RegionId = this.RegionId;
			result.X = this.X;
			result.Y = this.Y;
			result.IsAltar = this.IsAltar;

			result.Shapes = new List<ShapeData>(this.Shapes.Count);
			foreach (var item in this.Shapes)
				result.Shapes.Add(item.Copy());

			result.Parameters = new List<RegionElementData>(this.Parameters.Count);
			foreach (var item in this.Parameters)
				result.Parameters.Add(item.Copy());

			return result;
		}
	}

	public class RegionElementData
	{
		public int EventType { get; set; }
		public int SignalType { get; set; }
		public string Name { get; set; }
		public XElement XML { get; set; }

		public RegionElementData Copy()
		{
			var result = new RegionElementData();
			result.EventType = this.EventType;
			result.SignalType = this.SignalType;
			result.Name = this.Name;
			result.XML = this.XML != null ? new XElement(this.XML) : null;

			return result;
		}
	}

	public class RegionInfoDb : DatabaseDatIndexed<int, RegionInfoData>
	{
		private Random _rnd = new Random(Environment.TickCount);

		/// <summary>
		/// Returns region with given name or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public RegionInfoData GetRegion(string name)
		{
			return this.Entries.Values.FirstOrDefault(a => a.Name.ToLower() == name.ToLower());
		}

		/// <summary>
		/// Returns random coordinates inside the actual region.
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		public Point RandomCoord(int region)
		{
			var ri = this.Find(region);
			if (ri == null)
				return new Point();

			lock (_rnd)
				return ri.RandomCoord(_rnd);
		}

		/// <summary>
		/// Returns area id for the given location, or 0 if no area exists.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int GetAreaId(int region, int x, int y)
		{
			var ri = this.Find(region);
			if (ri == null)
				return int.MaxValue;

			return ri.GetAreaId(x, y);
		}

		/// <summary>
		/// Returns group id for the given region.
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public int GetGroupId(int regionId)
		{
			var data = this.Find(regionId);
			if (data == null)
				return -1;

			return data.GroupId;
		}

		/// <summary>
		/// Returns a list of events that start with the given path,
		/// e.g. "Uladh_main/field_Tir_S_aa/fish_tircho_stream_", to get all
		/// fishing events starting with that name.
		/// </summary>
		/// <param name="eventPath"></param>
		/// <returns></returns>
		public List<EventData> GetMatchingEvents(string eventPath)
		{
			var split = eventPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (split.Length != 3)
				throw new ArgumentException("Invalid event path, expected 3 segments.");

			var region = this.GetRegion(split[0]);
			if (region == null)
				throw new ArgumentException("Unknown region '" + split[0] + "'.");

			var area = region.GetArea(split[1]);
			if (area == null)
				throw new ArgumentException("Unknown area '" + split[1] + "' in region '" + split[0] + "'.");

			// TODO: Cache
			var result = new List<EventData>(area.Events.Values.Where(a => a.Name.StartsWith(split[2])));

			return result;
		}

		/// <summary>
		/// Loads data.
		/// </summary>
		/// <param name="br"></param>
		protected override void Read(BinaryReader br)
		{
			var cRegions = br.ReadInt32();
			for (int l = 0; l < cRegions; ++l)
			{
				var ri = new RegionInfoData();

				ri.Id = br.ReadInt32();
				ri.Name = br.ReadString();
				ri.GroupId = br.ReadInt32();
				ri.X1 = br.ReadInt32();
				ri.Y1 = br.ReadInt32();
				ri.X2 = br.ReadInt32();
				ri.Y2 = br.ReadInt32();

				var cAreas = br.ReadInt32();
				for (int i = 0; i < cAreas; ++i)
				{
					var ai = new AreaData();

					ai.Id = br.ReadInt32();
					ai.Name = br.ReadString();
					ai.X1 = br.ReadInt32();
					ai.Y1 = br.ReadInt32();
					ai.X2 = br.ReadInt32();
					ai.Y2 = br.ReadInt32();

					var cProps = br.ReadInt32();
					for (int j = 0; j < cProps; ++j)
					{
						var pi = new PropData();
						pi.EntityId = br.ReadInt64();
						pi.Id = br.ReadInt32();
						pi.Name = br.ReadString();
						pi.X = br.ReadSingle();
						pi.Y = br.ReadSingle();
						pi.Direction = br.ReadSingle();
						pi.Scale = br.ReadSingle();

						var cShapes = br.ReadInt32();
						for (int k = 0; k < cShapes; ++k)
						{
							var si = new ShapeData();
							si.X1 = br.ReadInt32();
							si.Y1 = br.ReadInt32();
							si.X2 = br.ReadInt32();
							si.Y2 = br.ReadInt32();
							si.X3 = br.ReadInt32();
							si.Y3 = br.ReadInt32();
							si.X4 = br.ReadInt32();
							si.Y4 = br.ReadInt32();

							pi.Shapes.Add(si);
						}

						var cElements = br.ReadInt32();
						for (int k = 0; k < cElements; ++k)
						{
							var red = new RegionElementData();
							red.EventType = br.ReadInt32();
							red.SignalType = br.ReadInt32();
							red.Name = br.ReadString();

							var xml = br.ReadString();
							red.XML = !string.IsNullOrWhiteSpace(xml) ? XElement.Parse(xml) : null;

							pi.Parameters.Add(red);
						}

						ai.Props.Add(pi.EntityId, pi);
					}

					var cEvents = br.ReadInt32();
					for (int j = 0; j < cEvents; ++j)
					{
						var ei = new EventData();
						ei.Id = br.ReadInt64();
						ei.Name = br.ReadString();
						ei.RegionId = ri.Id;
						ei.X = br.ReadSingle();
						ei.Y = br.ReadSingle();
						ei.Type = (EventType)br.ReadInt32();

						var cShapes = br.ReadInt32();
						for (int k = 0; k < cShapes; ++k)
						{
							var si = new ShapeData();
							si.X1 = br.ReadInt32();
							si.Y1 = br.ReadInt32();
							si.X2 = br.ReadInt32();
							si.Y2 = br.ReadInt32();
							si.X3 = br.ReadInt32();
							si.Y3 = br.ReadInt32();
							si.X4 = br.ReadInt32();
							si.Y4 = br.ReadInt32();

							ei.Shapes.Add(si);
						}

						var cElements = br.ReadInt32();
						for (int k = 0; k < cElements; ++k)
						{
							var red = new RegionElementData();
							red.EventType = br.ReadInt32();
							red.SignalType = br.ReadInt32();
							red.Name = br.ReadString();

							var xml = br.ReadString();
							red.XML = !string.IsNullOrWhiteSpace(xml) ? XElement.Parse(xml) : null;

							if (!ei.IsAltar && red.EventType == 2110 && red.SignalType == 103)
								ei.IsAltar = true;

							ei.Parameters.Add(red);
						}

						ai.Events.Add(ei.Id, ei);
					}

					ri.Areas.Add(ai);
				}

				this.Entries.Add(ri.Id, ri);
			}
		}
	}
}
