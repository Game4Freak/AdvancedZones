using System.Xml.Serialization;

namespace Game4Freak.AdvancedZones
{
    public class HeightNode
    {
        [XmlElement("x")]
        public float x;
        [XmlElement("z")]
        public float z;
        [XmlElement("y")]
        public float y;
        [XmlElement("isUpper")]
        public bool isUpper;
        
        public HeightNode()
        {
        }

        public HeightNode(float nX, float nZ, float nY, bool nIsUpper)
        {
            x = nX;
            z = nZ;
            y = nY;
            isUpper = nIsUpper;
        }
    }
}
