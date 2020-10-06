
namespace CheckInWpf
{
    class ListItem
    {
        public ListItem(string content)
        {
            ShowContent = content;
        }
        public ListItem(string content,bool check)
        {
            ShowContent = content;
            Checked = check;
        }
        public ListItem(string content,string id)
        {
            ID = id;
            ShowContent = content;
        }
        public ListItem(string content,string id,bool check)
        {
            ShowContent = content;
            ID = id;
            Checked = check;
        }
        public string ShowContent { get; set; }
        public string ID { get; set; }
        public bool Checked { get; set; }

        public override string ToString()
        {
            return ShowContent;
        }

        public string GetID()
        {
            return ID;
        }
    }
}
