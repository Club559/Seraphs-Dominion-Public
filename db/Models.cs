using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using db;

[Serializable, XmlRoot]
public class Chars
{
    private XmlSerializerNamespaces _namespaces;

    public Chars()
    {
        _namespaces = new XmlSerializerNamespaces(new[]
        {
            new XmlQualifiedName(string.Empty, "rotmg")
        });
    }

    [XmlElement("Char")]
    public List<Char> Characters { get; set; }

    [XmlAttribute("nextCharId")]
    public int NextCharId { get; set; }

    [XmlAttribute("maxNumChars")]
    public int MaxNumChars { get; set; }

    public Account Account { get; set; }

    [XmlArray("News")]
    [XmlArrayItem("Item")]
    public List<NewsItem> News { get; set; }

    [XmlArray("Servers")]
    [XmlArrayItem("Server")]
    public List<ServerItem> Servers { get; set; }

    public double? Lat { get; set; }
    public double? Long { get; set; }

    [XmlArray("ClassAvailabilityList")]
    [XmlArrayItem("ClassAvailability")]
    public List<ClassAvailabilityItem> ClassAvailabilityList { get; set; }

    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces Namespaces
    {
        get { return _namespaces; }
    }
}

[Serializable, XmlRoot]
public class Account
{
    public int Rank;
    private XmlSerializerNamespaces _namespaces;

    public Account()
    {
        _namespaces = new XmlSerializerNamespaces(new[]
        {
            new XmlQualifiedName(string.Empty, "rotmg")
        });
    }

    public int AccountId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    [XmlElement("NameChosen")]
    public string _NameChosen { get; set; }

    [XmlIgnore]
    public bool NameChosen
    {
        get { return _NameChosen != null; }
        set { _NameChosen = value ? "True" : null; }
    }

    [XmlElement("Converted")]
    public string _Converted { get; set; }

    [XmlIgnore]
    public bool Converted
    {
        get { return _Converted != null; }
        set { _Converted = value ? "True" : null; }
    }

    [XmlIgnore] //[XmlElement("StarredAccounts")]
    public string _StarredAccounts { get; set; }

    [XmlIgnore]
    public List<int> Locked
    {
        get
        {
            return (_StarredAccounts != null
                ? Utils.StringListToIntList(_StarredAccounts.Split(',').ToList())
                : new List<int>());
        }
        set { _StarredAccounts = string.Join(",", value); }
    }

    [XmlElement("IgnoredAccounts")]
    public string _IgnoredAccounts { get; set; }

    [XmlIgnore]
    public List<int> Ignored
    {
        get
        {
            return (_IgnoredAccounts != null
                ? Utils.StringListToIntList(_IgnoredAccounts.Split(',').ToList())
                : new List<int>());
        }
        set { _IgnoredAccounts = string.Join(",", value); }
    }

    [XmlElement("Admin")]
    public string _Admin { get; set; }

    [XmlIgnore]
    public bool Admin
    {
        get { return _Admin != null; }
        set { _Admin = value ? "True" : null; }
    }

    [XmlElement("Tag")]
    public string Tag { get; set; }

    [XmlElement("Mute")]
    public bool Muted { get; set; }

    [XmlElement("VerifiedEmail")]
    public string _VerifiedEmail { get; set; }

    [XmlIgnore]
    public bool VerifiedEmail
    {
        get { return _VerifiedEmail != null; }
        set { _VerifiedEmail = value ? "True" : null; }
    }

    [XmlElement("isGuest")]
    public string _isGuest { get; set; }

    [XmlIgnore]
    public bool isGuest
    {
        get { return _isGuest != null; }
        set { _isGuest = value ? "True" : null; }
    }

    [XmlElement("isBanned")]
    public string _isBanned { get; set; }

    [XmlIgnore]
    public bool isBanned
    {
        get { return _isBanned != null; }
        set { _isBanned = value ? "True" : null; }
    }

    public int Credits { get; set; }
    public int Souls { get; set; }
    public int NextCharSlotPrice { get; set; }
    public int BeginnerPackageTimeLeft { get; set; }

    public VaultData Vault { get; set; }
    public Stats Stats { get; set; }
    public Guild Guild { get; set; }

    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces Namespaces
    {
        get { return _namespaces; }
    }
}

[Serializable, XmlRoot]
public class VaultData
{
    [XmlElement("Chest")]
    public List<VaultChest> Chests { get; set; }
}

[Serializable, XmlRoot("VaultData")]
public class VaultChest
{
    [XmlIgnore]
    public int ChestId { get; set; }

    [XmlElement("Items")]
    public string _Items { get; set; }

    [XmlIgnore]
    public ushort[] Items
    {
        get { return Utils.FromCommaSepString16(_Items); }
        set { _Items = Utils.GetCommaSepString(value); }
    }

    [XmlElement("Datas")]
    public string _Datas { get; set; }

    [XmlIgnore]
    public ItemData[] Datas
    {
        get { return ItemDataList.CreateData(_Datas); }
        set { _Datas = ItemDataList.GetJson(value); }
    }
}

[Serializable, XmlRoot]
public class Stats
{
    public int BestCharFame { get; set; }
    public int TotalFame { get; set; }
    public int Fame { get; set; }

    [XmlElement("ClassStats")]
    public List<ClassStats> ClassStates { get; set; }
}

[Serializable, XmlRoot]
public class Guild
{
    [XmlAttribute("id")]
    public int Id { get; set; }

    public string Name { get; set; }
    public int Rank { get; set; }
    public int Fame { get; set; }
}

[Serializable, XmlRoot]
public class GuildStruct
{
    [XmlAttribute("id")]
    public int Id { get; set; }

    public string Name { get; set; }
    public int Level { get; set; }
    public string[] Members { get; set; }
    public int GuildFame { get; set; }
    public int TotalGuildFame { get; set; }
}

[Serializable, XmlRoot]
public class ClassStats
{
    [XmlAttribute("objectType")]
    public int ObjectType { get; set; }

    public int BestLevel { get; set; }
    public int BestFame { get; set; }
}

[Serializable, XmlRoot("Item")]
public class NewsItem
{
    public string Icon { get; set; }
    public string Title { get; set; }
    public string TagLine { get; set; }
    public string Link { get; set; }
    public int Date { get; set; }
}

[Serializable, XmlRoot("Server")]
public class ServerItem
{
    public string Name { get; set; }
    public string DNS { get; set; }
    public double Lat { get; set; }
    public double Long { get; set; }
    public double Usage { get; set; }

    [XmlElement("AdminOnly")]
    private string _AdminOnly { get; set; }

    [XmlIgnore]
    public bool AdminOnly
    {
        get { return _AdminOnly != null; }
        set { _AdminOnly = value ? "True" : null; }
    }
}

[Serializable, XmlRoot("Char")]
public class Char
{
    [XmlAttribute("id")]
    public int CharacterId { get; set; }

    public int ObjectType { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int CurrentFame { get; set; }

    [XmlElement("Equipment")]
    public string _Equipment { get; set; }

    [XmlIgnore]
    public ushort[] Equipment
    {
        get { return Utils.FromCommaSepString16(_Equipment); }
        set { _Equipment = Utils.GetCommaSepString(value); }
    }

    [XmlElement("EquipData")]
    public string _EquipData { get; set; }

    [XmlIgnore]
    public ItemData[] EquipData
    {
        get { return ItemDataList.CreateData(_EquipData); }
        set { _EquipData = ItemDataList.GetJson(value); }
    }

    public int MaxHitPoints { get; set; }
    public int HitPoints { get; set; }
    public int MaxMagicPoints { get; set; }
    public int MagicPoints { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
    public int Dexterity { get; set; }
    public int HpRegen { get; set; }
    public int MpRegen { get; set; }
    public int Tex1 { get; set; }
    public int Tex2 { get; set; }
    public int? XpBoost { get; set; }
    public int Skin { get; set; }
    public int PermaSkin { get; set; }
    public string Effect { get; set; }
    public string PCStats { get; set; }

    [XmlIgnore]
    public FameStats FameStats { get; set; }

    public bool Dead { get; set; }
    public int Pet { get; set; }
    public int Floors { get; set; }
}

[Serializable]
public class ClassAvailabilityItem
{
    [XmlAttribute("id")]
    public string Class { get; set; }

    [XmlText]
    public string Restricted { get; set; }
}