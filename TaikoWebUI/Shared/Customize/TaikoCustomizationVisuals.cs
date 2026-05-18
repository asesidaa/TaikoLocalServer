namespace TaikoWebUI.Shared.Customize;

public static class TaikoCustomizationVisuals
{
    private static readonly string[] CostumeColorValues =
    {
        "#F84828", "#68C0C0", "#DC1500", "#F8F0E0", "#009687", "#00BF87",
        "#00FF9A", "#66FFC2", "#FFFFFF", "#690000", "#FF0000", "#FF6666",
        "#FFB3B3", "#00BCC2", "#00F7FF", "#66FAFF", "#B3FDFF", "#E4E4E4",
        "#993800", "#FF5E00", "#FF9E78", "#FFCFB3", "#005199", "#0088FF",
        "#66B8FF", "#B3DBFF", "#B9B9B9", "#B37700", "#FFAA00", "#FFCC66",
        "#FFE2B3", "#000C80", "#0019FF", "#6675FF", "#B3BAFF", "#858585",
        "#B39B00", "#FFDD00", "#FFFF00", "#FFFF71", "#2B0080", "#5500FF",
        "#9966FF", "#CCB3FF", "#505050", "#38A100", "#78C900", "#B3FF00",
        "#DCFF8A", "#610080", "#C400FF", "#DC66FF", "#EDB3FF", "#232323",
        "#006600", "#00B800", "#00FF00", "#8AFF9E", "#990059", "#FF0095",
        "#FF66BF", "#FFB3DF", "#000000"
    };

    private static readonly string[] CostumeColorFilterValues =
    {
        "invert(48%) sepia(61%) saturate(6409%) hue-rotate(347deg) brightness(100%) contrast(95%);",
        "invert(90%) sepia(15%) saturate(1318%) hue-rotate(127deg) brightness(82%) contrast(78%);",
        "invert(15%) sepia(55%) saturate(6677%) hue-rotate(360deg) brightness(96%) contrast(102%);",
        "invert(88%) sepia(26%) saturate(147%) hue-rotate(343deg) brightness(106%) contrast(95%);",
        "invert(36%) sepia(47%) saturate(2136%) hue-rotate(146deg) brightness(97%) contrast(102%);",
        "invert(66%) sepia(50%) saturate(5209%) hue-rotate(124deg) brightness(92%) contrast(104%);",
        "invert(95%) sepia(83%) saturate(2827%) hue-rotate(73deg) brightness(101%) contrast(117%);",
        "invert(80%) sepia(39%) saturate(531%) hue-rotate(96deg) brightness(102%) contrast(101%);",
        "invert(100%);",
        "invert(12%) sepia(33%) saturate(6542%) hue-rotate(349deg) brightness(81%) contrast(117%);",
        "invert(15%) sepia(82%) saturate(6245%) hue-rotate(356deg) brightness(96%) contrast(125%);",
        "invert(51%) sepia(16%) saturate(2006%) hue-rotate(312deg) brightness(100%) contrast(100%);",
        "invert(97%) sepia(17%) saturate(5337%) hue-rotate(290deg) brightness(102%) contrast(100%);",
        "invert(52%) sepia(70%) saturate(2785%) hue-rotate(146deg) brightness(99%) contrast(105%);",
        "invert(84%) sepia(45%) saturate(4224%) hue-rotate(130deg) brightness(104%) contrast(103%);",
        "invert(82%) sepia(59%) saturate(1110%) hue-rotate(159deg) brightness(108%) contrast(104%);",
        "invert(85%) sepia(31%) saturate(483%) hue-rotate(158deg) brightness(104%) contrast(106%);",
        "invert(99%) sepia(69%) saturate(147%) hue-rotate(166deg) brightness(109%) contrast(79%);",
        "invert(17%) sepia(60%) saturate(5053%) hue-rotate(34deg) brightness(92%) contrast(101%);",
        "invert(53%) sepia(81%) saturate(4073%) hue-rotate(357deg) brightness(97%) contrast(112%);",
        "invert(66%) sepia(42%) saturate(573%) hue-rotate(326deg) brightness(101%) contrast(100%);",
        "invert(92%) sepia(9%) saturate(2479%) hue-rotate(306deg) brightness(105%) contrast(101%);",
        "invert(16%) sepia(70%) saturate(3568%) hue-rotate(195deg) brightness(95%) contrast(101%);",
        "invert(45%) sepia(67%) saturate(5622%) hue-rotate(193deg) brightness(104%) contrast(104%);",
        "invert(58%) sepia(74%) saturate(400%) hue-rotate(175deg) brightness(102%) contrast(102%);",
        "invert(78%) sepia(13%) saturate(856%) hue-rotate(177deg) brightness(104%) contrast(104%);",
        "invert(87%) sepia(0%) saturate(228%) hue-rotate(132deg) brightness(98%) contrast(66%);",
        "invert(52%) sepia(87%) saturate(3669%) hue-rotate(24deg) brightness(93%) contrast(100%);",
        "invert(63%) sepia(90%) saturate(1616%) hue-rotate(0deg) brightness(103%) contrast(105%);",
        "invert(74%) sepia(67%) saturate(370%) hue-rotate(346deg) brightness(102%) contrast(101%);",
        "invert(84%) sepia(32%) saturate(380%) hue-rotate(336deg) brightness(102%) contrast(106%);",
        "invert(11%) sepia(62%) saturate(7483%) hue-rotate(240deg) brightness(61%) contrast(117%);",
        "invert(13%) sepia(100%) saturate(4871%) hue-rotate(240deg) brightness(97%) contrast(143%);",
        "invert(68%) sepia(71%) saturate(6691%) hue-rotate(218deg) brightness(103%) contrast(100%);",
        "invert(76%) sepia(23%) saturate(1472%) hue-rotate(193deg) brightness(101%) contrast(103%);",
        "invert(55%) sepia(0%) saturate(428%) hue-rotate(151deg) brightness(96%) contrast(89%);",
        "invert(68%) sepia(42%) saturate(5979%) hue-rotate(24deg) brightness(97%) contrast(100%);",
        "invert(93%) sepia(14%) saturate(7042%) hue-rotate(356deg) brightness(103%) contrast(106%);",
        "invert(95%) sepia(23%) saturate(7492%) hue-rotate(1deg) brightness(107%) contrast(100%);",
        "invert(94%) sepia(94%) saturate(558%) hue-rotate(350deg) brightness(104%) contrast(107%);",
        "invert(15%) sepia(60%) saturate(5776%) hue-rotate(260deg) brightness(54%) contrast(123%);",
        "invert(17%) sepia(93%) saturate(5007%) hue-rotate(257deg) brightness(95%) contrast(157%);",
        "invert(46%) sepia(50%) saturate(3860%) hue-rotate(233deg) brightness(101%) contrast(102%);",
        "invert(71%) sepia(12%) saturate(1421%) hue-rotate(213deg) brightness(103%) contrast(102%);",
        "invert(29%) sepia(2%) saturate(0%) hue-rotate(226deg) brightness(99%) contrast(86%);",
        "invert(41%) sepia(97%) saturate(1327%) hue-rotate(66deg) brightness(94%) contrast(103%);",
        "invert(66%) sepia(70%) saturate(1886%) hue-rotate(40deg) brightness(97%) contrast(102%);",
        "invert(75%) sepia(98%) saturate(877%) hue-rotate(24deg) brightness(107%) contrast(105%);",
        "invert(86%) sepia(25%) saturate(644%) hue-rotate(29deg) brightness(105%) contrast(103%);",
        "invert(16%) sepia(49%) saturate(6851%) hue-rotate(281deg) brightness(59%) contrast(114%);",
        "invert(14%) sepia(97%) saturate(4833%) hue-rotate(282deg) brightness(102%) contrast(125%);",
        "invert(60%) sepia(34%) saturate(5439%) hue-rotate(240deg) brightness(96%) contrast(109%);",
        "invert(79%) sepia(50%) saturate(779%) hue-rotate(200deg) brightness(99%) contrast(106%);",
        "invert(0%) sepia(9%) saturate(62%) hue-rotate(43deg) brightness(108%) contrast(73%);",
        "invert(17%) sepia(45%) saturate(5456%) hue-rotate(101deg) brightness(100%) contrast(104%);",
        "invert(42%) sepia(53%) saturate(3615%) hue-rotate(93deg) brightness(104%) contrast(107%);",
        "invert(50%) sepia(63%) saturate(3097%) hue-rotate(88deg) brightness(124%) contrast(125%);",
        "invert(74%) sepia(96%) saturate(217%) hue-rotate(70deg) brightness(101%) contrast(101%);",
        "invert(17%) sepia(44%) saturate(7325%) hue-rotate(313deg) brightness(65%) contrast(109%);",
        "invert(13%) sepia(81%) saturate(7488%) hue-rotate(318deg) brightness(104%) contrast(102%);",
        "invert(60%) sepia(84%) saturate(2326%) hue-rotate(293deg) brightness(102%) contrast(100%);",
        "invert(88%) sepia(96%) saturate(1040%) hue-rotate(281deg) brightness(104%) contrast(106%);",
        "invert(0%);"
    };

    public static IReadOnlyList<string> CostumeColors => CostumeColorValues;

    public static string GetCostumeColorFilter(uint colorId)
        => colorId < CostumeColorFilterValues.Length
            ? CostumeColorFilterValues[colorId]
            : CostumeColorFilterValues[0];
}
