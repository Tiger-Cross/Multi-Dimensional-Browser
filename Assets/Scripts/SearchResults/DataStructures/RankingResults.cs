using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.SearchResults.DataStructures
{

    [Serializable]
    public class RankingResponse
    {
        public RankingGroup mainline;
        public RankingGroup pole;
        public RankingGroup sidebar;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Mainline results\n");
            sb.Append(mainline.ToString());
            sb.Append("Pole results\n");
            sb.Append(pole.ToString());
            sb.Append("Sidebar results\n");
            sb.Append(sidebar.ToString());
            return sb.ToString();
        }
    }

    [Serializable]
    public class RankingGroup
    {
        public List<RankingItem> items;

        public override string ToString()
        {
            if (items != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (RankingItem item in items)
                {
                    sb.AppendLine(item.ToString());
                }
                return sb.ToString();
            }
            return "";
        }
    }

    [Serializable]
    public class RankingItem
    {
        public string answerType;
        public string resultIndex;
        public string value;

        public override string ToString()
        {
            return $"( \n\tid : {value} ,\n\tindex: {resultIndex},\n\ttype: {answerType})";
        }
    }
}