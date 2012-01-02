﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JMMClient.ViewModel
{
	public class Trakt_ShoutUserVM
	{
		public int AnimeID { get; set; }

		// user details
		public Trakt_UserVM User { get; set; }
		// shout details
		public Trakt_ShoutVM Shout { get; set; }

		public Trakt_ShoutUserVM(JMMServerBinary.Contract_Trakt_ShoutUser contract)
		{
			this.User = new Trakt_UserVM(contract.User);
			this.Shout = new Trakt_ShoutVM(contract.Shout);
		}

		public string UserImagePathOnlineOrLocal
		{
			get
			{
				if (!string.IsNullOrEmpty(UserFullImagePath) && File.Exists(UserFullImagePath)) return UserFullImagePath;

				return UserOnlineImagePath;
			}
		}

		public string UserImagePathForDisplay
		{
			get
			{
				if (!string.IsNullOrEmpty(UserFullImagePath) && File.Exists(UserFullImagePath)) return UserFullImagePath;

				return @"/Images/EpisodeThumb_NotFound.png";
			}
		}

		public string UserOnlineImagePath
		{
			get
			{
				if (string.IsNullOrEmpty(User.Avatar)) return "";
				return User.Avatar;
			}
		}

		public string UserFullImagePath
		{
			get
			{
				// typical url
				// http://vicmackey.trakt.tv/images/avatars/837.jpg
				// http://gravatar.com/avatar/f894a4cbd5e8bcbb1a79010699af1183.jpg?s=140&r=pg&d=http%3A%2F%2Fvicmackey.trakt.tv%2Fimages%2Favatar-large.jpg

				if (string.IsNullOrEmpty(User.Avatar)) return "";

				string path = Utils.GetTraktImagePath_Avatars();
				return Path.Combine(path, string.Format("{0}.jpg", User.Username));
			}
		}
	}
}