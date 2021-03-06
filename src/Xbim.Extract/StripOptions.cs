using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xbim.IO.Step21;
using Xbim.IO.Step21.Step21;
using Xbim.IO.Step21.Step21.Text;
using Xbim.IO.Step21.Text;
using static Xbim.Extract.Program;

namespace Xbim.Extract
{
    [Verb("strip", HelpText = "Reduces ifc files retaining selected entities only. Example: strip file.ifc -e 1 23")]
    class StripOptions
    {
        [Value(0,
            MetaName = "input file",
            HelpText = "Input file to be processed.",
            Required = true)]
        public string FileName { get; set; }

        [Option(
            'e', "entities",
            HelpText = "list of entities to be retained."
            )]
        public IEnumerable<string> Entities { get; set; }

        [Option(
            's', "spatial",
            Default = false, Required = false,
            HelpText = "Produce a spatial structure for the IfcEntities."
            )]
        public bool Spatial { get; set; }

        [Option(
            'r', "referencing",
            HelpText = "search for ifcEntities referencing the provided list."
            )]
        public IEnumerable<string> Referencing { get; set; }

        internal static Status Run(StripOptions opts)
        {
            var f = new FileInfo(opts.FileName);
            if (!f.Exists)
            {
                Console.WriteLine($"File not found: {f.FullName}");
                return Status.NotFoundError;
            }

            try
            {
                var requiredEntities = new Queue<uint>(getEnt(opts.Entities, "Invalid entity id:'{0}'."));
                var referencing = new HashSet<uint>(getEnt(opts.Referencing, "Invalid referencing entity id:'{0}'."));
                var elements = new HashSet<uint>();
                var sites = new List<uint>();
                var ownerhist = new List<uint>();
                var mainplacements = new List<uint>();

                var totL = (double)f.Length;
                var headers = new List<TextSpan>();
                var entitySpans = new Dictionary<uint, TextSpan>();
                int maxEntitySize = -1;
                uint projectId = 0;

                // parse the entire file to get the addresses
                //
                void OnHeaderFound(StepHeaderEntity headerEntity)
                {
                    headers.Add(headerEntity.Span);
                    maxEntitySize = Math.Max(maxEntitySize, headerEntity.Span.Length);
                }
                using (var progress = new ProgressBar())
                {
                    void OnEntityBareFound(StepEntityAssignmentBare assignment)
                    {
                        var id = Convert.ToUInt32(assignment.Identity.Value);
                        entitySpans.Add(id, assignment.Span);
                        maxEntitySize = Math.Max(maxEntitySize, assignment.Span.Length);

                        var type = assignment.ExpressType.Text.ToLowerInvariant();
                        if (opts.Spatial && IsIfcElement(type))
                            elements.Add(id);
                        if (type == "ifcproject")
                        {
                            projectId = id;
                            if (!requiredEntities.Contains(id))
                                requiredEntities.Enqueue(id);
                        }
                        var perc = assignment.Span.End / totL;
                        progress.Report(perc);
                    }

                    // on entity found is only used if "referencing" is populated
                    void OnEntityFound(StepEntityAssignment assignment)
                    {
                        var id = Convert.ToUInt32(assignment.Identity.Value);
                        entitySpans.Add(id, assignment.Span);
                        maxEntitySize = Math.Max(maxEntitySize, assignment.Span.Length);
                        var type = assignment.ExpressType.Text.ToLowerInvariant();
                        if (opts.Spatial && IsIfcElement(type))
                            elements.Add(id);
                        if (type == "ifcproject")
                        {
                            projectId = id;
                            if (!requiredEntities.Contains(id))
                                requiredEntities.Enqueue(id);
                        }
                        if (opts.Spatial)
                        {
                            if (type == "ifcsite")
                                sites.Add(id);
                            if (type == "ifcownerhistory")
                                ownerhist.Add(id);
                            if (type == "ifclocalplacement")
                            {
                                var first = assignment.Entity.Attributes.FirstOrDefault();
                                if (first.Kind == StepKind.StepUndefined)
                                    mainplacements.Add(id);
                            }
                        }
                    

                        // if any of this entity references are requested, then this needs to
                        // be included.
                        foreach (var item in GetReferences(assignment.Entity.AttributesList))
                        {
                            if (referencing.Contains(item))
                            {
                                // in any case include this
                                if (!requiredEntities.Contains(id))
                                    requiredEntities.Enqueue(id);
                                // but only add this to the 'referencing' list if we have not
                                // reached a product
                                if (!IsIfcElement(assignment.ExpressType.Text) && !referencing.Contains(id))
                                    referencing.Add(id);
                            }
                        }

                        var perc = assignment.Span.End / totL;
                        progress.Report(perc);
                    }
                    using var st = new BufferedUri(f);
                    if (referencing.Any() || opts.Spatial)
                        StepParsing.ParseWithEvents(st, OnHeaderFound, OnEntityFound);
                    else
                        StepParsing.ParseWithEvents(st, OnHeaderFound, OnEntityBareFound);
                }
                if (projectId == 0)
                {
                    Console.WriteLine("Warning: IfcProject not found.");
                    opts.Spatial = false;
                }

                var done = new HashSet<uint>();
                var doneEntities = new List<uint>();
                // now get the dependencies
                //
                var buf = new char[maxEntitySize];
                var outName = f.FullName + ".stripped.ifc";
                if (File.Exists(outName))
                    File.Delete(outName);
                using var fw = File.CreateText(outName);
                using var fr = f.OpenText();
                fw.WriteLine("ISO-10303-21;");
                fw.WriteLine("HEADER;");
                foreach (var header in headers)
                {
                    CopySpan(buf, fw, fr, header);
                }
                fw.WriteLine("ENDSEC;");
                fw.WriteLine("DATA;");
                while (requiredEntities.Any())
                {
                    var requiredEntity = requiredEntities.Dequeue();

                    // wether succesful or not still add to done, to avoid repeating
                    done.Add(requiredEntity);
                    if (elements.Contains(requiredEntity))
                        doneEntities.Add(requiredEntity);
                    if (entitySpans.TryGetValue(requiredEntity, out var span))
                    {
                        CopySpan(buf, fw, fr, span);
                        var uRef = new Uri(f.FullName);
                        var asgn = StepParsing.ParseEntityAssignment(new string(buf), uRef);
                        var refs = GetReferences(asgn.Entity.AttributesList).ToList();
                        foreach (var reference in refs)
                        {
                            if (done.Contains(reference))
                                continue;
                            if (requiredEntities.Contains(reference))
                                continue;
                            requiredEntities.Enqueue(reference);
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine($"Error: entity #{requiredEntity} not found.");
                    }
                }
                
                if (opts.Spatial)
                {
                    uint max = done.Max();
                    var ownerhistlabel = done.Intersect(ownerhist).FirstOrDefault();
                    var exportedSite = done.Intersect(sites).FirstOrDefault();
                    var mainPlacement = done.Intersect(mainplacements).FirstOrDefault();
                    if (exportedSite == 0)
                    {
                        exportedSite = ++max;
                        fw.WriteLine($"#{exportedSite}= IFCSITE('{newguid()}',#{ownerhistlabel},'XbimCreated','Created with the stripping method of Xbim Simplify','',#{mainPlacement},$,$,.ELEMENT.,$,$,0.,$,$);");
                    }
                    // make site part of project.
                    fw.WriteLine($"#{++max}= IFCRELAGGREGATES('{newguid()}',#{ownerhistlabel},$,$,#{projectId},(#{exportedSite}));");

                    // make elements part of site
                    var exportedElements = done.Intersect(elements);
                    foreach (var element in exportedElements)
                    {
                        fw.WriteLine($"#{++max}= IFCRELCONTAINEDINSPATIALSTRUCTURE('{newguid()}',#{ownerhistlabel},$,$,(#{element}),#{exportedSite});");
                    }
                }
                fw.WriteLine("ENDSEC;");
                fw.WriteLine("END-ISO-10303-21;");
            }
            catch (Exception ex)
            {
                var c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.Write(ex.Message);
                Console.ForegroundColor = c;
                return Status.Exception;
            }
            Console.WriteLine("Completed.");
            return Status.Ok;
        }

        private static string newguid()
        {
            var g = Guid.NewGuid();
            return GuidFunctions.ConvertToBase64(g);
        }

        private static bool IsIfcElement(string text)
        {
            return text.ToLowerInvariant() switch
            {
                "ifcfooting" or
                "ifcpile" or
                "ifcbeam" or
                "ifcbeamstandardcase" or
                "ifcbuildingelementproxy" or
                "ifcchimney" or
                "ifccolumn" or
                "ifccolumnstandardcase" or
                "ifccovering" or
                "ifccurtainwall" or
                "ifcdoor" or
                "ifcdoorstandardcase" or
                "ifcmember" or
                "ifcmemberstandardcase" or
                "ifcplate" or
                "ifcplatestandardcase" or
                "ifcrailing" or
                "ifcramp" or
                "ifcrampflight" or
                "ifcroof" or
                "ifcshadingdevice" or
                "ifcslab" or
                "ifcslabelementedcase" or
                "ifcslabstandardcase" or
                "ifcstair" or
                "ifcstairflight" or
                "ifcwall" or
                "ifcwallelementedcase" or
                "ifcwallstandardcase" or
                "ifcwindow" or
                "ifcwindowstandardcase" or
                "ifcreinforcingbar" or
                "ifcreinforcingmesh" or
                "ifctendon" or
                "ifctendonanchor" or
                "ifcbuildingelementpart" or
                "ifcdiscreteaccessory" or
                "ifcfastener" or
                "ifcmechanicalfastener" or
                "ifcvibrationisolator" or
                "ifcsurfacefeature" or
                "ifcvoidingfeature" or
                "ifcopeningelement" or
                "ifcopeningstandardcase" or
                "ifcprojectionelement" or
                "ifcfurnishingelement" or
                "ifcfurniture" or
                "ifcsystemfurnitureelement" or
                "ifcdistributionelement" or
                "ifcdistributionflowelement" or
                "ifcdistributionchamberelement" or
                "ifcenergyconversiondevice" or
                "ifcairtoairheatrecovery" or
                "ifcboiler" or
                "ifcburner" or
                "ifcchiller" or
                "ifccoil" or
                "ifccondenser" or
                "ifccooledbeam" or
                "ifccoolingtower" or
                "ifcengine" or
                "ifcevaporativecooler" or
                "ifcevaporator" or
                "ifcheatexchanger" or
                "ifchumidifier" or
                "ifctubebundle" or
                "ifcunitaryequipment" or
                "ifcelectricgenerator" or
                "ifcelectricmotor" or
                "ifcmotorconnection" or
                "ifcsolardevice" or
                "ifctransformer" or
                "ifcflowcontroller" or
                "ifcairterminalbox" or
                "ifcdamper" or
                "ifcflowmeter" or
                "ifcvalve" or
                "ifcelectricdistributionboard" or
                "ifcelectrictimecontrol" or
                "ifcprotectivedevice" or
                "ifcswitchingdevice" or
                "ifcflowfitting" or
                "ifcductfitting" or
                "ifcpipefitting" or
                "ifccablecarrierfitting" or
                "ifccablefitting" or
                "ifcjunctionbox" or
                "ifcflowmovingdevice" or
                "ifccompressor" or
                "ifcfan" or
                "ifcpump" or
                "ifcflowsegment" or
                "ifcductsegment" or
                "ifcpipesegment" or
                "ifccablecarriersegment" or
                "ifccablesegment" or
                "ifcflowstoragedevice" or
                "ifctank" or
                "ifcelectricflowstoragedevice" or
                "ifcflowterminal" or
                "ifcfiresuppressionterminal" or
                "ifcsanitaryterminal" or
                "ifcstackterminal" or
                "ifcwasteterminal" or
                "ifcairterminal" or
                "ifcmedicaldevice" or
                "ifcspaceheater" or
                "ifcaudiovisualappliance" or
                "ifccommunicationsappliance" or
                "ifcelectricappliance" or
                "ifclamp" or
                "ifclightfixture" or
                "ifcoutlet" or
                "ifcflowtreatmentdevice" or
                "ifcinterceptor" or
                "ifcductsilencer" or
                "ifcfilter" or
                "ifcdistributioncontrolelement" or
                "ifcprotectivedevicetrippingunit" or
                "ifcactuator" or
                "ifcalarm" or
                "ifccontroller" or
                "ifcflowinstrument" or
                "ifcsensor" or
                "ifcunitarycontrolelement" or
                "ifccivilelement" or
                "ifcelementassembly" or
                "ifcgeographicelement" or
                "ifctransportelement" or
                "ifcvirtualelement" or
                "ifcelectricdistributionpoint" or
                "ifcchamferedgefeature" or
                "ifcroundededgefeature" or
                "ifcelectricalelement" or
                "ifcequipmentelement" => true,
                _ => false,
            };
        }

        private static IEnumerable<uint> getEnt(IEnumerable<string> entities, string message)
        {
            foreach (var item in entities)
            {
                if (uint.TryParse(item, out var converted))
                {
                    yield return converted;
                }
                else
                {
                    throw new Exception(string.Format(message, item));
                }
            }
        }

        private static void CopySpan(char[] buf, StreamWriter fw, StreamReader fr, TextSpan span)
        {
            fr.BaseStream.Seek(span.Start, SeekOrigin.Begin);
            fr.DiscardBufferedData();
            fr.Read(buf, 0, span.Length);
            fw.WriteLine(buf, 0, span.Length);
        }

        private static IEnumerable<uint> GetReferences(StepAttributeList attributesList)
        {
            foreach (var att in attributesList.Attributes)
            {
                if (att.Kind == StepKind.StepArgumentsList && att is StepAttributeList al)
                {
                    foreach (var sub in GetReferences(al))
                    {
                        yield return sub;
                    }
                }
                else if (att.Kind == StepKind.StepEntity && att is StepEntity ent)
                {
                    foreach (var sub in GetReferences(ent.AttributesList))
                    {
                        yield return sub;
                    }
                }
                else if (att.Kind == StepKind.StepIdentityToken && att is StepToken st)
                {
                    yield return Convert.ToUInt32(st.Value);
                }
            }
        }
    }
}
