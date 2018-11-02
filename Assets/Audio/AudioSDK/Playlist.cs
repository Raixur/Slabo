using System;

namespace AudioSDK
{
    [Serializable]
    public class Playlist
    {
        public string Name;
        public string[] PlaylistItems;

        public Playlist()
        {
            Name = "Default";
            PlaylistItems = null;
        }

        public Playlist(string name, string[] playlistItems)
        {
            Name = name;
            PlaylistItems = playlistItems;
        }
    }
}
