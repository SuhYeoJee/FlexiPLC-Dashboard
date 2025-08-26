namespace FlexiPLC.Core.Models
{
    public class PlcItem
    {   // from config.json ["Items"] 
        public string Name { get; set; }        //ex) label
        public string Address { get; set; }     //ex) D100
        public string DataType { get; set; }    //ex) int
    }
}