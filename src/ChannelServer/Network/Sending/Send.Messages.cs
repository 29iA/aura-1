﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Channel.World.Entities;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends system message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void SystemMessage(Creature creature, string format, params object[] args)
		{
			SystemMessage(creature, "<SYSTEM>", format, args);
		}

		/// <summary>
		/// Sends server message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void ServerMessage(Creature creature, string format, params object[] args)
		{
			SystemMessage(creature, "<SERVER>", format, args);
		}

		/// <summary>
		/// Sends combat message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void CombatMessage(Creature creature, string format, params object[] args)
		{
			SystemMessage(creature, "<COMBAT>", format, args);
		}

		/// <summary>
		/// Sends system message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		private static void SystemMessage(Creature creature, string from, string format, params object[] args)
		{
			var packet = new Packet(Op.Chat, creature.EntityId);
			packet.PutByte(0);
			packet.PutString(from);
			packet.PutString(format, args);
			packet.PutByte(true);
			packet.PutUInt(0xFFFF8080);
			packet.PutInt(0);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends Chat in range of creature.
		/// </summary>
		/// <param name="creature">Source, in terms of name and position</param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Chat(Creature creature, string format, params object[] args)
		{
			var packet = new Packet(Op.Chat, creature.EntityId);
			packet.PutByte(0); // speech (0) vs thought (1) bubble
			packet.PutString(creature.Name);
			packet.PutString(format, args);

			// The following part is not required for normal chat

			packet.PutByte(0); // 1 hides chat bubble and enables --v
			packet.PutUInt(0); // custom color (supports alpha transparency, FF is 100% opaque)

			packet.PutInt(0);
			packet.PutByte(0);

			creature.Region.Broadcast(packet, creature);
		}
	}
}
