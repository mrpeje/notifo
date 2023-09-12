using System.Xml.Serialization;

namespace Notifo.Domain.Integrations.SMSGyteways.Gyteways.XML;

[XmlType(TypeName = "response")]
public sealed class XMLResponse
{
    [XmlElement("information")]
    public Information Information { get; set; }

    [XmlElement("error")]
    public string Error { get; set; }

    [XmlIgnore]
    public bool Sent { get; set; } = false;
}

public sealed class Information
{
    [XmlAttribute("number_sms")]
    public string NumberSMS { get; set; }

    [XmlAttribute("id_sms")]
    public int IdSMS { get; set; }

    [XmlAttribute("parts")]
    public int Parts { get; set; }

    [XmlText]
    public string Status { get; set; }
}
