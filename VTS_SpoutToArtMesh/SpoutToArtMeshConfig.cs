using System.Collections.Generic;

namespace VTS_SpoutToArtMesh
{
    public class SpoutToArtMeshConfig
    {
        public string SpoutName;
        public bool HorizontalFlip = true;
        public bool VerticalFlip;
        public List<string> ArtMeshNames = new List<string>();
    }
}