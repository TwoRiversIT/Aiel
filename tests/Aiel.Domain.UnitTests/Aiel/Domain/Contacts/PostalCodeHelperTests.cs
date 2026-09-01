// MIT License
//
// Copyright 2026 Two Rivers Information Technology Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sub-license,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using Aiel.Domain.Geography;
using System.Diagnostics;

namespace Aiel.Domain.Contacts;

public class PostCodeHelperTests
{
    private static readonly String[] Codes = ["00501", "00544", "01001", "01002", "01003", "01004", "01005", "01007", "01008", "01009", "01010", "01011", "01012", "01013", "01014", "01020", "01021", "01022", "01026", "01027", "01028", "01029", "01030", "01031", "01032", "01033", "01034", "01035", "01036", "01037", "01038", "01039", "01040", "01041", "01050", "01053", "01054", "01056", "01057", "01059", "01060", "01061", "01062", "01063", "01066", "01068", "01069", "01070", "01071", "01072", "01073", "01074", "01075", "01077", "01079", "01080", "01081", "01082", "01083", "01084", "01085", "01086", "01088", "01089", "01090", "01092", "01093", "01094", "01095", "01096", "01097", "01098", "01101", "01102", "01103", "01104", "01105", "01106", "01107", "01108", "01109", "01111", "01115", "01116", "01118", "01119", "01128", "01129", "01138", "01139", "01144", "01151", "01152", "01199", "01201", "01202", "01203", "01220", "01222", "01223", "01224", "01225", "01226", "01227", "01229", "01230", "01235", "01236", "01237", "01238", "01240", "01242", "01243", "01244", "01245", "01247", "01252", "01253", "01254", "01255", "01256", "01257", "01258", "01259", "01260", "01262", "01263", "01264", "01266", "01267", "01270", "01301", "01302", "01330", "01331", "01337", "01338", "01339", "01340", "01341", "01342", "01343", "01344", "01346", "01347", "01349", "01350", "01351", "01354", "01355", "01360", "01364", "01366", "01367", "01368", "01370", "01373", "01375", "01376", "01378", "01379", "01380", "01420", "01430", "01431", "01432", "01434", "01436", "01438", "01440", "01441", "01450", "01451", "01452", "01453", "01460", "01462", "01463", "01464", "01467", "01468", "01469", "01470", "01471", "01472", "01473", "01474", "01475", "01477", "01501", "01503", "01504", "01505", "01506", "01507", "01508", "01509", "01510", "01515", "01516", "01517", "01518", "01519", "01520", "01521", "01522", "01523", "01524", "01525", "01526", "01527", "01529", "01531", "01532", "01534", "01535", "01536", "01537", "01538", "01540", "01541", "01542", "01543", "01545", "01546", "01550", "01560", "01561", "01562", "01564", "01566", "01568", "01569", "01570", "01571", "01580", "01581", "01582", "01583", "01585", "01586", "01588", "01590", "01601", "01602", "01603", "01604", "01605", "01606", "01607", "01608", "01609", "01610", "01611", "01612", "01613", "01614", "01615", "01653", "01654", "01655", "01701", "01702", "01703", "01704", "01705", "01718", "01719", "01720", "01721", "01730", "01731", "01740", "01741", "01742", "01745", "01746", "01747", "01748", "01749", "01752", "01754", "01756", "01757", "01760", "01770", "01772", "01773", "01775", "01776", "01778", "01784", "01801", "01803", "01805", "01807", "01810", "01812", "01813", "01815", "01821", "01822", "01824", "01826", "01827", "01830", "01831", "01832", "01833", "01834", "01835", "01840", "01841", "01842", "01843", "01844", "01845", "01850", "01851", "01852", "01853", "01854", "01860", "01862", "01863", "01864", "01865", "01866", "01867", "01876", "01879", "01880", "01885", "01886", "01887", "01888", "01889", "01890", "01899", "01901", "01902", "01903", "01904", "01905", "01906", "01907", "01908", "01910", "01913", "01915", "01921", "01922", "01923", "01929", "01930", "01931", "01936", "01937", "01938", "01940", "01944", "01945", "01949", "01950", "01951", "01952", "01960", "01961", "01965", "01966", "01969", "01970", "01971", "01982", "01983", "01984", "01985", "02018", "02019", "02020", "02021", "02025", "02026", "02027", "02030", "02032", "02035", "02038", "02040", "02041", "02043", "02044", "02045", "02047", "02048", "02050", "02051", "02052", "02053", "02054", "02055", "02056", "02059", "02060", "02061", "02062", "02065", "02066", "02067", "02070", "02071", "02072", "02081", "02090", "02093", "02108", "02109", "02110", "02111", "02112", "02113", "02114", "02115", "02116", "02117", "02118", "02119", "02120", "02121", "02122", "02123", "02124", "02125", "02126", "02127", "02128", "02129", "02130", "02131", "02132", "02133", "02134", "02135", "02136", "02137", "02138", "02139", "02140", "02141", "02142", "02143", "02144", "02145", "02148", "02149", "02150", "02151", "02152", "02153", "02155", "02156", "02163", "02169", "02170", "02171", "02176", "02180", "02184", "02185", "02186", "02187", "02188", "02189", "02190", "02191", "02196", "02199", "02201", "02203", "02204", "02205", "02206", "02210", "02211", "02212", "02215", "02217", "02222", "02228", "02238", "02241", "02266", "02269", "02283", "02284", "02293", "02295", "02297", "02298", "02301", "02302", "02303", "02304", "02305", "02322", "02324", "02325", "02327", "02330", "02331", "02332", "02333", "02334", "02337", "02338", "02339", "02340", "02341", "02343", "02344", "02345", "02346", "02347", "02348", "02349", "02350", "02351", "02355", "02356", "02357", "02358", "02359", "02360", "02361", "02362", "02364", "02366", "02367", "02368", "02370", "02375", "02379", "02381", "02382", "02420", "02421", "02445", "02446", "02447", "02451", "02452", "02453", "02454", "02455", "02456", "02457", "02458", "02459", "02460", "02461", "02462", "02464", "02465", "02466", "02467", "02468", "02471", "02472", "02474", "02475", "02476", "02477", "02478", "02479", "02481", "02482", "02492", "02493", "02494", "02495", "02532", "02534", "02535", "02536", "02537", "02538", "02539", "02540", "02541", "02542", "02543", "02552", "02553", "02554", "02556", "02557", "02558", "02559", "02561", "02562", "02563", "02564", "02565", "02568", "02571", "02573", "02574", "02575", "02576", "02584", "02601", "02630", "02631", "02632", "02633", "02634", "02635", "02637", "02638", "02639", "02641", "02642", "02643", "02644", "02645", "02646", "02647", "02648", "02649", "02650", "02651", "02652", "02653", "02655", "02657", "02659", "02660", "02661", "02662", "02663", "02664", "02666", "02667", "02668", "02669", "02670", "02671", "02672", "02673", "02675", "02702", "02703", "02712", "02713", "02714", "02715", "02717", "02718", "02719", "02720", "02721", "02722", "02723", "02724", "02725", "02726", "02738", "02739", "02740", "02741", "02742", "02743", "02744", "02745", "02746", "02747", "02748", "02760", "02761", "02762", "02763", "02764", "02766", "02767", "02768", "02769", "02770", "02771", "02777", "02779", "02780", "02783", "02790", "02791", "02801", "02802", "02804", "02806", "02807", "02808", "02809", "02812", "02813", "02814", "02815", "02816", "02817", "02818", "02822", "02823", "02824", "02825", "02826", "02827", "02828", "02829", "02830", "02831", "02832", "02833", "02835", "02836", "02837", "02838", "02839", "02840", "02841", "02842", "02852", "02857", "02858", "02859", "02860", "02861", "02862", "02863", "02864", "02865", "02871", "02872", "02873", "02874", "02875", "02876", "02877", "02878", "02879", "02880", "02881", "02882", "02883", "02885", "02886", "02887", "02888", "02889", "02891", "02892", "02893", "02894", "02895", "02896", "02898", "02901", "02902", "02903", "02904", "02905", "02906", "02907", "02908", "02909", "02910", "02911", "02912", "02914", "02915", "02916", "02917", "02918", "02919", "02920", "02921", "02940", "03031", "03032", "03033", "03034", "03036", "03037", "03038", "03040", "03041", "03042", "03043", "03044", "03045", "03046", "03047", "03048", "03049", "03051", "03052", "03053", "03054", "03055", "03057", "03060", "03061", "03062", "03063", "03064", "03070", "03071", "03073", "03076", "03077", "03079", "03082", "03084", "03086", "03087", "03101", "03102", "03103", "03104", "03105", "03106", "03107", "03108", "03109", "03110", "03111", "03215", "03216", "03217", "03218", "03220", "03221", "03222", "03223", "03224", "03225", "03226", "03227", "03229", "03230", "03231", "03233", "03234", "03235", "03237", "03238", "03240", "03241", "03242", "03243", "03244", "03245", "03246", "03247", "03249", "03251", "03252", "03253", "03254", "03255", "03256", "03257", "03258", "03259", "03260", "03261", "03262", "03263", "03264", "03266", "03268", "03269", "03272", "03273", "03274", "03275", "03276", "03278", "03279", "03280", "03281", "03282", "03284", "03285", "03287", "03289", "03290", "03291", "03293", "03298", "03299", "03301", "03302", "03303", "03304", "03305", "03307", "03431", "03435", "03440", "03441", "03442", "03443", "03444", "03445", "03446", "03447", "03448", "03449", "03450", "03451", "03452", "03455", "03456", "03457", "03458", "03461", "03462", "03464", "03465", "03466", "03467", "03468", "03469", "03470", "03561", "03570", "03574", "03575", "03576", "03579", "03580", "03581", "03582", "03583", "03584", "03585", "03586", "03588", "03589", "03590", "03592", "03593", "03595", "03597", "03598", "03601", "03602", "03603", "03604", "03605", "03607", "03608", "03609", "03740", "03741", "03743", "03745", "03746", "03748", "03749", "03750", "03751", "03752", "03753", "03754", "03755", "03756", "03765", "03766", "03768", "03769", "03770", "03771", "03773", "03774", "03777", "03779", "03780", "03781", "03782", "03784", "03785", "03801", "03802", "03803", "03804", "03805", "03809", "03810", "03811", "03812", "03813", "A0A 1A0", "A0A 1B0", "A0A 1C0", "A0A 1E0", "A0A 1G0", "A0A 1H0", "A0A 1J0", "A0A 1K0", "A0A 1L0", "A0A 1M0", "A0A 1N0", "A0A 1P0", "A0A 1R0", "A0A 1S0", "A0A 1V0", "A0A 1W0", "A0A 1X0", "A0A 1Y0", "A0A 1Z0", "A0A 2B0", "A0A 2G0", "A0A 2H0", "A0A 2L0", "A0A 2M0", "A0A 2N0", "A0A 2P0", "A0A 2R0", "A0A 2S0", "A0A 2V0", "A0A 2W0", "A0A 2X0", "A0A 2Z0", "A0A 3A0", "A0A 3B0", "A0A 3C0", "A0A 3E0", "A0A 3G0", "A0A 3H0", "A0A 3J0", "A0A 3L0", "A0A 3M0", "A0A 3N0", "A0A 3P0", "A0A 3R0", "A0A 3S0", "A0A 3V0", "A0A 3W0", "A0A 3X0", "A0A 3X1", "A0A 4A0", "A0A 4B0", "A0A 4E0", "A0A 4G0", "A0A 4H0", "A0A 4J0", "A0A 4K0", "A0A 4L0", "A0B 1A0", "A0B 1B0", "A0B 1C0", "A0B 1E0", "A0B 1G0", "A0B 1H0", "A0B 1J0", "A0B 1K0", "A0B 1L0", "A0B 1M0", "A0B 1N0", "A0B 1P0", "A0B 1R0", "A0B 1S0", "A0B 1T0", "A0B 1V0", "A0B 1W0", "A0B 1X0", "A0B 1Y0", "A0B 1Z0", "A0B 2A0", "A0B 2B0", "A0B 2C0", "A0B 2E0", "A0B 2G0", "A0B 2H0", "A0B 2J0", "A0B 2L0", "A0B 2M0", "A0B 2N0", "A0B 2P0", "A0B 2R0", "A0B 2S0", "A0B 2T0", "A0B 2V0", "A0B 2W0", "A0B 2Y0", "A0B 2Z0", "A0B 3A0", "A0B 3B0", "A0B 3C0", "A0B 3E0", "A0B 3H0", "A0B 3J0", "A0B 3K0", "A0B 3L0", "A0B 3M0", "A0B 3P0", "A0C 1A0", "A0C 1B0", "A0C 1E0", "A0C 1G0", "A0C 1H0", "A0C 1J0", "A0C 1K0", "A0C 1L0", "A0C 1M0", "A0C 1N0", "A0C 1P0", "A0C 1R0", "A0C 1S0", "A0C 1T0", "A0C 1V0", "A0C 1W0", "A0C 1Y0", "A0C 1Z0", "A0C 2A0", "A0C 2B0", "A0C 2C0", "A0C 2E0", "A0C 2G0", "A0C 2H0", "A0C 2J0", "A0C 2K0", "A0C 2M0", "A0C 2N0", "A0C 2P0", "A0C 2R0", "A0C 2S0", "A0E 1A0", "A0E 1B0", "A0E 1C0", "A0E 1E0", "A0E 1G0", "A0E 1H0", "A0E 1K0", "A0E 1L0", "A0E 1M0", "A0E 1N0", "A0E 1P0", "A0E 1R0", "A0E 1S0", "A0E 1T0", "A0E 1V0", "A0E 1W0", "A0E 1X0", "A0E 1Y0", "A0E 1Z0", "A0E 2A0", "A0E 2B0", "A0E 2C0", "A0E 2E0", "A0E 2G0", "A0E 2H0", "A0E 2J0", "A0E 2K0", "A0E 2L0", "A0E 2M0", "A0E 2N0", "A0E 2P0", "A0E 2R0", "A0E 2S0", "A0E 2T0", "A0E 2V0", "A0E 2W0", "A0E 2X0", "A0E 2Y0", "A0E 2Z0", "A0E 3A0", "A0E 3B0", "A0G 1A0", "A0G 1B0", "A0G 1C0", "A0G 1E0", "A0G 1G0", "A0G 1H0", "A0G 1J0", "A0G 1K0", "A0G 1L0", "A0G 1M0", "A0G 1N0", "A0G 1P0", "A0G 1R0", "A0G 1S0", "A0G 1T0", "A0G 1V0", "A0G 1W0", "A0G 1X0", "A0G 1Y0", "A0G 1Z0", "A0G 2A0", "A0G 2B0", "A0G 2C0", "A0G 2E0", "A0G 2G0", "A0G 2H0", "A0G 2J0", "A0G 2K0", "A0G 2L0", "A0G 2M0", "A0G 2N0", "A0G 2P0", "A0G 2R0", "A0G 2S0", "A0G 2T0", "A0G 2V0", "A0G 2W0", "A0G 2X0", "A0G 2Y0", "A0G 2Z0", "A0G 3A0", "A0G 3B0", "A0G 3C0", "A0G 3E0", "A0G 3G0", "A0G 3H0", "A0G 3J0", "A0G 3K0", "A0G 3L0", "A0G 3M0", "A0G 3N0", "A0G 3P0", "A0G 3R0", "A0G 3S0", "A0G 3T0", "A0G 3V0", "A0G 3W0", "A0G 3X0", "A0G 3Y0", "A0G 3Z0", "A0G 4A0", "A0G 4B0", "A0G 4C0", "A0G 4E0", "A0G 4G0", "A0G 4H0", "A0G 4J0", "A0G 4K0", "A0G 4L0", "A0G 4M0", "A0G 4N0", "A0G 4P0", "A0G 4R0", "A0G 4S0", "A0G 4T0", "A0H 1A0", "A0H 1B0", "A0H 1C0", "A0H 1E0", "A0H 1G0", "A0H 1H0", "A0H 1J0", "A0H 1L0", "A0H 1M0", "A0H 1N0", "A0H 1P0", "A0H 1R0", "A0H 1S0", "A0H 1T0", "A0H 1V0", "A0H 1W0", "A0H 1Y0", "A0H 1Z0", "A0H 2A0", "A0H 2B0", "A0H 2C0", "A0H 2E0", "A0H 2G0", "A0H 2J0", "A0J 1A0", "A0J 1B0", "A0J 1E0", "A0J 1G0", "A0J 1H0", "A0J 1J0", "A0J 1K0", "A0J 1L0", "A0J 1M0", "A0J 1N0", "A0J 1P0", "A0J 1R0", "A0J 1S0", "A0J 1T0", "A0J 1V0", "A0K 1A0", "A0K 1B0", "A0K 1C0", "A0K 1H0", "A0K 1J0", "A0K 1K0", "A0K 1L0", "A0K 1M0", "A0K 1N0", "A0K 1P0", "A0K 1R0", "A0K 1S0", "A0K 1T0", "A0K 1V0", "A0K 1W0", "A0K 1X0", "A0K 1Y0", "A0K 1Z0", "A0K 2A0", "A0K 2B0", "A0K 2C0", "A0K 2G0", "A0K 2H0", "A0K 2J0", "A0K 2M0", "A0K 2N0", "A0K 2P0", "A0K 2V0", "A0K 2W0", "A0K 2X0", "A0K 2Y0", "A0K 3A0", "A0K 3B0", "A0K 3E0", "A0K 3H0", "A0K 3K0", "A0K 3L0", "A0K 3M0", "A0K 3N0", "A0K 3P0", "A0K 3R0", "A0K 3S0", "A0K 3T0", "A0K 3V0", "A0K 3X0", "A0K 3Y0", "A0K 3Z0", "A0K 4A0", "A0K 4B0", "A0K 4C0", "A0K 4E0", "A0K 4G0", "A0K 4H0", "A0K 4J0", "A0K 4K0", "A0K 4L0", "A0K 4M0", "A0K 4N0", "A0K 4P0", "A0K 4R0", "A0K 4S0", "A0K 4T0", "A0K 4V0", "A0K 4W0", "A0K 4Y0", "A0K 4Z0", "A0K 5C0", "A0K 5E0", "A0K 5G0", "A0K 5H0", "A0K 5K0", "A0K 5P0", "A0K 5R0", "A0K 5S0", "A0K 5T0", "A0K 5V0", "A0K 5X0", "A0K 5Y0", "A0L 1A0", "A0L 1C0", "A0L 1E0", "A0L 1G0", "A0L 1H0", "A0L 1J0", "A0L 1K0", "A0L 1L0", "A0M 1B0", "A0M 1C0", "A0M 1G0", "A0M 1J0", "A0M 1K0", "A0M 1P0", "A0N 1A0", "A0N 1B0", "A0N 1C0", "A0N 1E0", "A0N 1G0", "A0N 1H0", "A0N 1J0", "A0N 1K0", "A0N 1M0", "A0N 1N0", "A0N 1P0", "A0N 1R0", "A0N 1S0", "A0N 1T0", "A0N 1T1", "A0N 1V0", "A0N 1W0", "A0N 1X0", "A0N 1Y0", "A0N 1Z0", "A0N 2B0", "A0N 2C0", "A0N 2E0", "A0N 2G0", "A0N 2H0", "A0N 2J0", "A0N 2K0", "A0N 2L0", "A0P 1A0", "A0P 1C0", "A0P 1E0", "A0P 1G0", "A0P 1J0", "A0P 1K0", "A0P 1L0", "A0P 1M0", "A0P 1N0", "A0P 1P0", "A0P 1S0", "A0R 1A0", "A0R 1B0", "A1A 0A1", "A1A 0A2", "A1A 0A3", "A1A 0A4", "A1A 0A5", "A1A 0A6", "A1A 0A7", "A1A 0A8", "A1A 0A9", "A1A 0B1", "A1A 0B2", "A1A 0B3", "A1A 0B4", "A1A 0B5", "A1A 0B6", "A1A 0B7", "A1A 0B8", "A1A 0B9", "A1A 0C1", "A1A 0C2", "A1A 0C3", "A1A 0C4", "A1A 0C5", "A1A 0C6", "A1A 0C7", "A1A 0C8", "A1A 0C9", "A1A 0E1", "A1A 0E2", "A1A 0E3", "A1A 0E4", "A1A 0E5", "A1A 0E6", "A1A 0E7", "A1A 0E8", "A1A 0G1", "A1A 0G2", "A1A 0G3", "A1A 0G4", "A1A 0G5", "A1A 0G6", "A1A 0G7", "A1A 0G8", "A1A 0G9", "A1A 0H1", "A1A 0H2", "A1A 0H3", "A1A 0H4", "A1A 0H5", "A1A 0H6", "A1A 0H7", "A1A 0H8", "A1A 0H9", "A1A 0J1", "A1A 0J2", "A1A 0J3", "A1A 0J4", "A1A 0J5", "A1A 0J6", "A1A 0J7", "A1A 0J8", "A1A 0J9", "A1A 0K1", "A1A 0K2", "A1A 0K3", "A1A 0K4", "A1A 0K5", "A1A 0K6", "A1A 0K7", "A1A 0K8", "A1A 0K9", "A1A 0L1", "A1A 0L2", "A1A 0L3", "A1A 0L4", "A1A 0L5", "A1A 0L6", "A1A 0L7", "A1A 0L8", "A1A 1A1", "A1A 1A2", "A1A 1A3", "A1A 1A4", "A1A 1A5", "A1A 1A6", "A1A 1A7", "A1A 1A8", "A1A 1A9", "A1A 1B1", "A1A 1B2", "A1A 1B3", "A1A 1B4", "A1A 1B5", "A1A 1B6", "A1A 1B7", "A1A 1B8", "A1A 1B9", "A1A 1C1", "A1A 1C2", "A1A 1C3", "A1A 1C4", "A1A 1C5", "A1A 1C6", "A1A 1C7", "A1A 1C8", "A1A 1C9", "A1A 1E1", "A1A 1E2", "A1A 1E3", "A1A 1E4", "A1A 1E5", "A1A 1E6", "A1A 1E7", "A1A 1E8", "A1A 1E9", "A1A 1G1", "A1A 1G2", "A1A 1G3", "A1A 1G4", "A1A 1G5", "A1A 1G7", "A1A 1G8", "A1A 1G9", "A1A 1H1", "A1A 1H2", "A1A 1H3", "A1A 1H4", "A1A 1H5", "A1A 1H7", "A1A 1H8", "A1A 1H9", "A1A 1J1", "A1A 1J2", "A1A 1J3", "A1A 1J4", "A1A 1J5", "A1A 1J6", "A1A 1J7", "A1A 1J8", "A1A 1J9", "A1A 1K1", "A1A 1K2", "A1A 1K3", "A1A 1K5", "A1A 1K6", "A1A 1K7", "A1A 1K8", "A1A 1K9", "A1A 1L1", "A1A 1L2", "A1A 1L3", "A1A 1L4", "A1A 1L5", "A1A 1L6", "A1A 1L7", "A1A 1L8", "A1A 1L9", "A1A 1M1", "A1A 1M2", "A1A 1M3", "A1A 1M4", "A1A 1M5", "A1A 1M6", "A1A 1M7", "A1A 1M8", "A1A 1M9", "A1A 1N3", "A1A 1N6", "A1A 1N7", "A1A 1P1", "A1A 1P7", "A1A 1P8", "A1A 1R1", "A1A 1R3", "A1A 1R4", "A1A 1R5", "A1A 1R6", "A1A 1R7", "A1A 1R8", "A1A 1R9", "A1A 1S1", "A1A 1S3", "A1A 1S4", "A1A 1S5", "A1A 1S6", "A1A 1S7", "A1A 1T2", "A1A 1T3", "A1A 1T4", "A1A 1T5", "A1A 1T6", "A1A 1T7", "A1A 1T8", "A1A 1T9", "A1A 1V1", "A1A 1V2", "A1A 1V3", "A1A 1V4", "A1A 1V5", "A1A 1V6", "A1A 1V7", "A1A 1V8", "A1A 1V9", "A1A 1W1", "A1A 1W2", "A1A 1W3", "A1A 1W4", "A1A 1W5", "A1A 1W6", "A1A 1W7", "A1A 1W8", "A1A 1W9", "A1A 1X1", "A1A 1X2", "A1A 1X3", "A1A 1X4", "A1A 1X5", "A1A 1X6", "A1A 1X7", "A1A 1X8", "A1A 1X9", "A1A 1Y1", "A1A 1Y2", "A1A 1Y3", "A1A 1Y4", "A1A 1Y5", "A1A 1Y6", "A1A 1Y7", "A1A 1Y8", "A1A 1Y9", "A1A 1Z1", "A1A 1Z2", "A1A 1Z3", "A1A 1Z4", "A1A 1Z5", "A1A 1Z6", "A1A 1Z7", "A1A 1Z8", "A1A 1Z9", "A1A 2A1", "A1A 2A2", "A1A 2A3", "A1A 2A4", "A1A 2A5", "A1A 2A6", "A1A 2A7", "A1A 2A8", "A1A 2A9", "A1A 2B1", "A1A 2B2", "A1A 2B3", "A1A 2B4", "A1A 2B5", "A1A 2B6", "A1A 2B7", "A1A 2B8", "A1A 2B9", "A1A 2C1", "A1A 2C2", "A1A 2C3", "A1A 2C4", "A1A 2C5", "A1A 2C7", "A1A 2C8", "A1A 2C9", "A1A 2E1", "A1A 2E2", "A1A 2E3", "A1A 2E4", "A1A 2E5", "A1A 2E6", "A1A 2E7", "A1A 2E8", "A1A 2E9", "A1A 2G1", "A1A 2G2", "A1A 2G3", "A1A 2G4", "A1A 2G5", "A1A 2G6", "A1A 2G7", "A1A 2G8", "A1A 2G9", "A1A 2H1", "A1A 2H3", "A1A 2H4", "A1A 2H6", "A1A 2H7", "A1A 2H8", "A1A 2H9", "A1A 2J1", "A1A 2J2", "A1A 2J3", "A1A 2J4", "A1A 2J5", "A1A 2J6", "A1A 2J7", "A1A 2J8", "A1A 2J9", "A1A 2K1", "A1A 2K2", "A1A 2K3", "A1A 2K4", "A1A 2K5", "A1A 2K6", "A1A 2K7", "A1A 2K8", "A1A 2K9", "A1A 2L1", "A1A 2L2", "A1A 2L3", "A1A 2L4", "A1A 2L5", "A1A 2L6", "A1A 2L7", "A1A 2L8", "A1A 2L9", "A1A 2M1", "A1A 2M2", "A1A 2M3", "A1A 2M4", "A1A 2M5", "A1A 2M6", "A1A 2M7", "A1A 2M8", "A1A 2M9", "A1A 2N1", "A1A 2N2", "A1A 2N3", "A1A 2N4", "A1A 2N5", "A1A 2N6", "A1A 2N7", "A1A 2N8", "A1A 2N9", "A1A 2P1", "A1A 2P2", "A1A 2P3", "A1A 2P4", "A1A 2P5", "A1A 2P6", "A1A 2P7", "A1A 2P8", "A1A 2P9", "A1A 2R1", "A1A 2R2", "A1A 2R3", "A1A 2R4", "A1A 2R5", "A1A 2R6", "A1A 2R7", "A1A 2R8", "A1A 2R9", "A1A 2S1", "A1A 2S2", "A1A 2S3", "A1A 2S4", "A1A 2S5", "A1A 2S6", "A1A 2S7", "A1A 2S8", "A1A 2S9", "A1A 2T1", "A1A 2T2", "A1A 2T3", "A1A 2T4", "A1A 2T5", "A1A 2T6", "A1A 2T7", "A1A 2T8", "A1A 2T9", "A1A 2V1", "A1A 2V2", "A1A 2V3", "A1A 2V4", "A1A 2V5", "A1A 2V6", "A1A 2V7", "A1A 2V8", "A1A 2V9", "A1A 2W1", "A1A 2W2", "A1A 2W3", "A1A 2W4", "A1A 2W5", "A1A 2W6", "A1A 2W7", "A1A 2W8", "A1A 2W9", "A1A 2X1", "A1A 2Y5", "A1A 2Y6", "A1A 2Y7", "A1A 2Y8", "A1A 2Y9", "A1A 2Z1", "A1A 2Z2", "A1A 2Z3", "A1A 2Z4", "A1A 2Z5", "A1A 2Z6", "A1A 2Z7", "A1A 2Z8", "A1A 2Z9", "A1A 3A1", "A1A 3A2", "A1A 3A3", "A1A 3A4", "A1A 3A5", "A1A 3A6", "A1A 3A7", "A1A 3A8", "A1A 3A9", "A1A 3B1", "A1A 3B2", "A1A 3B3", "A1A 3B4", "A1A 3B5", "A1A 3B7", "A1A 3B8", "A1A 3C1", "A1A 3C3", "A1A 3C4", "A1A 3C5", "A1A 3C6", "A1A 3C7", "A1A 3C8", "A1A 3C9", "A1A 3E1", "A1A 3E2", "A1A 3E3", "A1A 3E4", "A1A 3E5", "A1A 3E7", "A1A 3E8", "A1A 3E9", "A1A 3G1", "A1A 3G2", "A1A 3G3", "A1A 3G4", "A1A 3G5", "A1A 3G6", "A1A 3G7", "A1A 3G8", "A1A 3G9", "A1A 3H1", "A1A 3H2", "A1A 3H3", "A1A 3H4", "A1A 3H5", "A1A 3H6", "A1A 3H7", "A1A 3H8", "A1A 3H9", "A1A 3J1", "A1A 3J2", "A1A 3J3", "A1A 3J4", "A1A 3J5", "A1A 3J6", "A1A 3J7", "A1A 3J8", "A1A 3J9", "A1A 3K1", "A1A 3K2", "A1A 3K3", "A1A 3K4", "A1A 3K5", "A1A 3K6", "A1A 3K7", "A1A 3K8", "A1A 3K9", "A1A 3L1", "A1A 3L2", "A1A 3L3", "A1A 3L4", "A1A 3L5", "A1A 3L6", "A1A 3L7", "A1A 3L8", "A1A 3L9", "A1A 3M1", "A1A 3M2", "A1A 3M3", "A1A 3M4", "A1A 3M5", "A1A 3M6", "A1A 3M7", "A1A 3M8", "A1A 3M9", "A1A 3N1", "A1A 3N2", "A1A 3N3", "A1A 3N4", "A1A 3N5", "A1A 3N6", "A1A 3N7", "A1A 3N8", "A1A 3N9", "A1A 3P1", "A1A 3P2", "A1A 3P3", "A1A 3P4", "A1A 3P5", "A1A 3P6", "A1A 3P7", "A1A 3P8", "A1A 3P9", "A1A 3R1", "A1A 3R2", "A1A 3R3", "A1A 3R4", "A1A 3R5", "A1A 3R6", "A1A 3R7", "A1A 3R8", "A1A 3R9", "A1A 3S1", "A1A 3S2", "A1A 3S3", "A1A 3S4", "A1A 3S5", "A1A 3S6", "A1A 3S8", "A1A 3S9", "A1A 3T1", "A1A 3T3", "A1A 3T5", "A1A 3T7", "A1A 3V1", "A1A 3V2", "A1A 3V3", "A1A 3V4", "A1A 3V5", "A1A 3V6", "A1A 3V9", "A1A 3W1", "A1A 3W2", "A1A 3W4", "A1A 3W5", "A1A 3W6", "A1A 3W7", "A1A 3W8", "A1A 3W9", "A1A 3X1", "A1A 3X2", "A1A 3X3", "A1A 3X4", "A1A 3X5", "A1A 3X6", "A1A 3X7", "A1A 3X9", "A1A 3Y1", "A1A 3Y2", "A1A 3Y4", "A1A 3Y6", "A1A 3Y7", "A1A 3Y8", "A1A 3Y9", "A1A 3Z1", "A1A 3Z2", "A1A 3Z3", "A1A 3Z4", "A1A 3Z5", "A1A 3Z6", "A1A 3Z7", "A1A 3Z9", "A1A 4A1", "A1A 4A2", "A1A 4A3", "A1A 4A4", "A1A 4A5", "A1A 4A6", "A1A 4A7", "A1A 4A8", "A1A 4A9"];

    public class IsValid
    {
        [Fact]
        public void Returns_false_for_null_empty_or_whitespace()
        {
            PostCodeHelper.IsValidPostCode(null!).Should().BeFalse();
            PostCodeHelper.IsValidPostCode("").Should().BeFalse();
            PostCodeHelper.IsValidPostCode(" ").Should().BeFalse();
        }
    }

    public class PostalCodes
    {
        [Fact]
        public void Performance()
        {
            var total = Stopwatch.StartNew();
            var counter = 0;
            for (var i = 0; i < 50; i++)
            {
                foreach (var code in Codes)
                {
                    counter++;
                    PostCodeHelper.IsValidPostCode(code);
                }
            }

            total.Stop();

            var avg = total.ElapsedMilliseconds / Convert.ToDouble(counter);
            Debug.WriteLine("{0} Postal Codes. Total Time: {1} ms  Average: {2} ms", counter, total.ElapsedMilliseconds, avg);
        }

        [Fact]
        public void AreInvalidWhenNullEmptyOrWhiteSpace()
        {
            PostCodeHelper.IsValidPostalCode((String)null!).Should().BeFalse();
            "".IsValidPostalCode().Should().BeFalse();
            " ".IsValidPostalCode().Should().BeFalse();
        }

        [Fact]
        public void AreFormattedLetterNumberLetterNumberLetterNumber()
        {
            "A0A0A0".IsValidPostalCode().Should().BeTrue();
            "0A0A0A".IsValidPostalCode().Should().BeFalse();
        }

        [Fact]
        public void AreInvalidWithLeadingOrTrailingWhiteSpace()
        {
            " A0A0A0".IsValidPostalCode().Should().BeFalse();
            "A0A0A0 ".IsValidPostalCode().Should().BeFalse();
            " A0A0A0 ".IsValidPostalCode().Should().BeFalse();
        }

        [Fact]
        public void AreInvalidWhenNonAlphaNumericCharactersArePresent()
        {
            "A!A0A0".IsValidPostalCode().Should().BeFalse();
            "!A0A0A0".IsValidPostalCode().Should().BeFalse();
            "A0A0A0.".IsValidPostalCode().Should().BeFalse();
        }

        [Fact]
        public void AreValidWithOrWithoutASpaceInTheMiddle()
        {
            "A0A0A0".IsValidPostalCode().Should().BeTrue();
            "A0A 0A0".IsValidPostalCode().Should().BeTrue();
            "A0A  0A0".IsValidPostalCode().Should().BeTrue();
        }

        [Fact]
        public void AreValidWithHyphenInTheMiddle()
        {
            "A0A-0A0".IsValidPostalCode().Should().BeTrue();
        }

        [Fact]
        public void AreInvalidWithMultipleHyphensInTheMiddle()
        {
            "A0A--0A0".IsValidPostalCode().Should().BeFalse();
        }

        [Fact]
        public void AreInvalidWithSpaceAndHyphenInTheMiddle()
        {
            "A0A- 0A0".IsValidPostalCode().Should().BeFalse();
            "A0A -0A0".IsValidPostalCode().Should().BeFalse();
        }

        [Fact]
        public void AreNotCaseSensitive()
        {
            "v0v0v0".IsValidPostalCode().Should().BeTrue();
            "V0V 0V0".IsValidPostalCode().Should().BeTrue();
        }

        [Fact]
        public void DoNotIncludeTheLettersDFIOQU()
        {
            "A0A0D0".IsValidPostalCode().Should().BeFalse();
            "A0A0F0".IsValidPostalCode().Should().BeFalse();
            "A0A0I0".IsValidPostalCode().Should().BeFalse();
            "A0A0O0".IsValidPostalCode().Should().BeFalse();
            "A0A0Q0".IsValidPostalCode().Should().BeFalse();
            "A0A0U0".IsValidPostalCode().Should().BeFalse();
        }

        [Fact]
        public void DoNotStartWithWorZ()
        {
            "W0A0A0".IsValidPostalCode().Should().BeFalse();
            "Z0A0F0".IsValidPostalCode().Should().BeFalse();
        }
    }

    public class PartialPostalCodes
    {
        [Fact]
        public void AreOneTwoOrThreeCharactersLong()
        {
            PostCodeHelper.IsValidPartialPostalCode("V").Should().BeTrue();
            PostCodeHelper.IsValidPartialPostalCode("V1").Should().BeTrue();
            PostCodeHelper.IsValidPartialPostalCode("V1V").Should().BeTrue();
            PostCodeHelper.IsValidPartialPostalCode("V1V1").Should().BeFalse();
            PostCodeHelper.IsValidPartialPostalCode("V1V1V").Should().BeFalse();
            PostCodeHelper.IsValidPartialPostalCode("V1V1V1").Should().BeFalse();
            PostCodeHelper.IsValidPartialPostalCode("V1V ").Should().BeFalse();
            PostCodeHelper.IsValidPartialPostalCode("V1V 1").Should().BeFalse();
            PostCodeHelper.IsValidPartialPostalCode("V1V 1V").Should().BeFalse();
            PostCodeHelper.IsValidPartialPostalCode("V1V 1V1").Should().BeFalse();
        }

        [Fact]
        public void AreNotNullEmptyOrWhiteSpace()
        {
            PostCodeHelper.IsValidPartialPostalCode(null!).Should().BeFalse();
            PostCodeHelper.IsValidPartialPostalCode("").Should().BeFalse();
            PostCodeHelper.IsValidPartialPostalCode(" ").Should().BeFalse();
        }
    }

    public class FsaCodeValidation
    {
        [Fact]
        public void AreInvalidWhenNullEmptyOrWhiteSpace()
        {
            PostCodeHelper.IsValidFsaCode(null!).Should().BeFalse();
            PostCodeHelper.IsValidFsaCode("").Should().BeFalse();
            PostCodeHelper.IsValidFsaCode(" ").Should().BeFalse();
        }

        [Fact]
        public void AreInvalidWithLeadingOrTrailingWhiteSpace()
        {
            PostCodeHelper.IsValidFsaCode(" A0A").Should().BeFalse();
            PostCodeHelper.IsValidFsaCode("A0A ").Should().BeFalse();
            PostCodeHelper.IsValidFsaCode(" A0A ").Should().BeFalse();
        }

        [Fact]
        public void AreNotCaseSensitive()
        {
            PostCodeHelper.IsValidFsaCode("a0a").Should().BeTrue();
            PostCodeHelper.IsValidFsaCode("A0A").Should().BeTrue();
        }

        [Fact]
        public void DoNotIncludeTheLettersDFIOQU()
        {
            PostCodeHelper.IsValidFsaCode("A0D").Should().BeFalse();
            PostCodeHelper.IsValidFsaCode("A0F").Should().BeFalse();
            PostCodeHelper.IsValidFsaCode("A0I").Should().BeFalse();
            PostCodeHelper.IsValidFsaCode("A0O").Should().BeFalse();
            PostCodeHelper.IsValidFsaCode("A0Q").Should().BeFalse();
            PostCodeHelper.IsValidFsaCode("A0U").Should().BeFalse();
        }

        [Fact]
        public void DoNotStartWithWorZ()
        {
            PostCodeHelper.IsValidFsaCode("W0A").Should().BeFalse();
            PostCodeHelper.IsValidFsaCode("Z0A").Should().BeFalse();
        }
    }

    public class ZipCodes
    {
        [Fact]
        public void AreFiveDigits()
        {
            "9021".IsValidZipCode().Should().BeFalse();
            "90210".IsValidZipCode().Should().BeTrue();
            "902100".IsValidZipCode().Should().BeFalse();
            "aaaaa".IsValidZipCode().Should().BeFalse();
        }

        [Fact]
        public void DoesNotSupportFiveDigitsPlusFourDigits()
        {
            "22222+0000".IsValidZipCode().Should().BeFalse();
            "22222 +0000".IsValidZipCode().Should().BeFalse();
            "22222+000".IsValidZipCode().Should().BeFalse();
            "22222+00000".IsValidZipCode().Should().BeFalse();
            "22222+abcde".IsValidZipCode().Should().BeFalse();
        }

        [Fact]
        public void AreInvalidWhenNullEmptyOrWhiteSpace()
        {
            PostCodeHelper.IsValidZipCode(null!).Should().BeFalse();
            "".IsValidZipCode().Should().BeFalse();
            " ".IsValidZipCode().Should().BeFalse();
        }

        [Fact]
        public void AreInvalidWithLeadingOrTrailingWhiteSpace()
        {
            " 90210".IsValidZipCode().Should().BeFalse();
            "90210 ".IsValidZipCode().Should().BeFalse();
            " 90210 ".IsValidZipCode().Should().BeFalse();
        }

        [Fact]
        public void AreValidWhenMatchingThePatternButAreAnInvalidCase()
        {
            "00000".IsValidZipCode().Should().BeFalse();
            "11111".IsValidZipCode().Should().BeFalse();
            "33333".IsValidZipCode().Should().BeFalse();
            "66666".IsValidZipCode().Should().BeFalse();
            "77777".IsValidZipCode().Should().BeFalse();
            "88888".IsValidZipCode().Should().BeFalse();
            "99999".IsValidZipCode().Should().BeFalse();
        }

        [Fact]
        public void AreInvalidWhenContainingNonNumericCharacters()
        {
            "@#$90210".IsValidZipCode().Should().BeFalse();
            "90210@#$".IsValidZipCode().Should().BeFalse();
            "@90210$".IsValidZipCode().Should().BeFalse();
        }
    }

    public class PartialZipCodes
    {
        [Fact]
        public void AreOneTwoOrThreeCharactersLong()
        {
            PostCodeHelper.IsValidPartialZipCode("2").Should().BeTrue();
            PostCodeHelper.IsValidPartialZipCode("22").Should().BeTrue();
            PostCodeHelper.IsValidPartialZipCode("222").Should().BeTrue();
            PostCodeHelper.IsValidPartialZipCode("2222").Should().BeFalse();
            PostCodeHelper.IsValidPartialZipCode("22222").Should().BeFalse();
        }

        [Fact]
        public void AreNotNullEmptyOrWhiteSpace()
        {
            PostCodeHelper.IsValidPartialZipCode(null!).Should().BeFalse();
            PostCodeHelper.IsValidPartialZipCode("").Should().BeFalse();
            PostCodeHelper.IsValidPartialZipCode(" ").Should().BeFalse();
        }
    }

    public class ScfCodeValidation
    {
        [Fact]
        public void AreThreeDigits()
        {
            PostCodeHelper.IsValidScfCode("00").Should().BeFalse();
            PostCodeHelper.IsValidScfCode("000").Should().BeTrue();
            PostCodeHelper.IsValidScfCode("0000").Should().BeFalse();
            PostCodeHelper.IsValidScfCode("aaa").Should().BeFalse();
        }

        [Fact]
        public void AreInvalidWhenNullEmptyOrWhiteSpace()
        {
            PostCodeHelper.IsValidScfCode(null!).Should().BeFalse();
            PostCodeHelper.IsValidScfCode("").Should().BeFalse();
            PostCodeHelper.IsValidScfCode(" ").Should().BeFalse();
        }

        [Fact]
        public void AreInvalidWithLeadingOrTrailingWhiteSpace()
        {
            PostCodeHelper.IsValidScfCode(" 000").Should().BeFalse();
            PostCodeHelper.IsValidScfCode("000 ").Should().BeFalse();
            PostCodeHelper.IsValidScfCode(" 000 ").Should().BeFalse();
        }
    }

    public class FormatCode
    {
        private const Int32 Iterations = 1000;
        private readonly ITestOutputHelper _output;

        public FormatCode(ITestOutputHelper output)
        {
            // Warmup
            foreach (var code in Codes)
            {
                PostCodeHelper.FormatCode(code);
                code.FormatPostalCode();
                PostCodeHelper.FormatCodeSlow(code);
            }

            _output = output;
        }

        [Fact]
        public void Performance_FormatCode()
        {
            var total = Stopwatch.StartNew();
            var counter = 0;
            for (var i = 0; i < Iterations; i++)
            {
                foreach (var code in Codes)
                {
                    counter++;
                    PostCodeHelper.FormatCode(code);
                }
            }

            total.Stop();

            var avg = total.ElapsedMilliseconds / Convert.ToDouble(counter);
            _output.WriteLine("{0} Postal Codes. Total Time: {1} ms  Average: {2} ms", counter, total.ElapsedMilliseconds, avg);
        }

        [Fact]
        public void Performance_FormatPostalCode()
        {
            var total = Stopwatch.StartNew();
            var counter = 0;
            for (var i = 0; i < Iterations; i++)
            {
                foreach (var code in Codes)
                {
                    counter++;
                    code.FormatPostalCode();
                }
            }

            total.Stop();

            var avg = total.ElapsedMilliseconds / Convert.ToDouble(counter);
            _output.WriteLine("{0} Postal Codes. Total Time: {1} ms  Average: {2} ms", counter, total.ElapsedMilliseconds, avg);
        }

        [Fact]
        public void Performance_FormatCodeSlow()
        {
            var total = Stopwatch.StartNew();
            var counter = 0;
            for (var i = 0; i < Iterations; i++)
            {
                foreach (var code in Codes)
                {
                    counter++;
                    PostCodeHelper.FormatCodeSlow(code);
                }
            }

            total.Stop();

            var avg = total.ElapsedMilliseconds / Convert.ToDouble(counter);
            _output.WriteLine("{0} Postal Codes. Total Time: {1} ms  Average: {2} ms", counter, total.ElapsedMilliseconds, avg);
        }

        [Fact]
        public void MustChangeLowerCaseToUpperCase()
        {
            PostCodeHelper.FormatCode("a0a 0a0").Should().Be("A0A 0A0");
            PostCodeHelper.FormatCode("v1v 1v1").Should().Be("V1V 1V1");
        }

        [Fact]
        public void MustInsertSpaceInTheMiddleOfPostalCode()
        {
            PostCodeHelper.FormatCode("A0A0A0").Should().Be("A0A 0A0");
        }

        [Fact]
        public void MustHandleZipCode()
        {
            PostCodeHelper.FormatCode("00000").Should().Be("00000");
        }

        [Fact]
        public void MustHandleHyphenInPostalCode()
        {
            PostCodeHelper.FormatCode("V1Y-3W1").Should().Be("V1Y 3W1");
        }

        [Fact]
        public void MustHandlePlusInPostalCode()
        {
            PostCodeHelper.FormatCode("V1Y+3W1").Should().Be("V1Y 3W1");
        }

        [Theory]
        [InlineData("v5k", "v5k")]
        [InlineData("abcdef", "abcdef")]
        [InlineData("ABCDEF", "ABCDEF")]
        [InlineData(" ABCDEF ", " ABCDEF ")]
        [InlineData("123456", "123456")]
        [InlineData("12345 ", "12345")]
        [InlineData("1 2 3 4 5", "12345")]
        [InlineData(" 11111 ", "11111")]
        [InlineData("1 1 1 1 1", "11111")]
        [InlineData(" A0A 0A0 ", "A0A 0A0")]
        public void MustNotModifyAnythingThatIsNotAZipCodeOrPostalCode(String? input, String? expected)
        {
            PostCodeHelper.FormatCode(input).Should().Be(expected);
        }

        [Theory]
        [InlineData((String)null!, "")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        [InlineData("\t", "")]
        public void Formats_NullOrWhitespace_as_EmptyString(String? input, String? expected)
        {
            PostCodeHelper.FormatCode(input).Should().Be(expected);
        }
    }
}
