using System.Xml.Serialization;

namespace Notifo.Domain.Integrations.SMSGyteways.Gyteways.XML;
[XmlType(TypeName = "request")]
public class XMLRequest
{
    [XmlElement("message")]
    public XMLRequestMessage Message { get; set; }
    [XmlElement("security")]
    public XMLRequestSecurity Security { get; set; }
}

public class XMLRequestMessage
{
    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlElement("sender")]
    public string Sender { get; set; }

    [XmlElement("text")]
    public string Text { get; set; }

    [XmlElement("abonent")]
    public XMLRequestSubscriber Subscriber { get; set; }
}

public class XMLRequestSubscriber
{
    [XmlAttribute("phone")]
    public string Phone { get; set; }

    [XmlAttribute("number_sms")]
    public int NumberSMS { get; set; }
}

public class XMLRequestSecurity
{
    [XmlElement("login")]
    public Login Login{get; set; }
    [XmlElement("password")]
    public Password Password { get; set; }
}

public class Login
{
    [XmlAttribute("value")]
    public string Value { get; set; }
}

public class Password
{
    [XmlAttribute("value")]
    public string Value { get; set; }
}
