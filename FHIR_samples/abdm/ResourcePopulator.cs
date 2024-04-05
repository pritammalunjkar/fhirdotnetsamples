using System;
using System.Collections.Generic;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Source;
using System.IO;
using Hl7.Fhir.Validation;
using System.Net;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Net.Http;

namespace FHIR_Profile_Validation
{
    public static class ResourcePopulator
    {
        public static bool seralize_WriteFile(string filename_IN, Base profile_IN)
        {
            bool isSuccess = true;
            bool inValidFileExtension = false;
            try
            {

                FhirJsonSerializer serializer = new FhirJsonSerializer(new SerializerSettings()
                {
                    Pretty = true,
                });

                FhirXmlSerializer serializerXML = new FhirXmlSerializer(new SerializerSettings()
                {
                    Pretty = true,
                });

                Console.WriteLine("\nEnter file path to write bundle (extension .json or .xml):");
                filename_IN = Console.ReadLine();
                string filepath = filename_IN;
                FileInfo fi = new FileInfo(filepath);

                // Serialize populated bundle to Json 
                if (fi.Extension == ".json")
                {
                    string bundeljson = serializer.SerializeToString(profile_IN);
                    File.WriteAllText(filepath, bundeljson);

                }
                else if (fi.Extension == ".xml")
                {
                    string bundelxml = serializerXML.SerializeToString(profile_IN);
                    File.WriteAllText(filepath, bundelxml);
                }
                else
                {
                    Console.WriteLine("Invalid file extension!");
                    isSuccess = false;
                    inValidFileExtension = true;
                }

                if (inValidFileExtension != true)
                {
                    // Parse the xml/json file
                    Base profile = null;
                    if (fi.Extension == ".json")
                    {
                        var parser = new FhirJsonParser();
                        profile = parser.Parse(File.ReadAllText(filepath));
                    }
                    else if (fi.Extension == ".xml")
                    {
                        var parser = new FhirXmlParser();
                        profile = parser.Parse(File.ReadAllText(filepath));
                    }
                    else
                    {
                        Console.WriteLine("Invalid file extension!");
                        isSuccess = false;
                    }

                    // Validate Parsed file
                    string strErr_OUT = "";
                    if (ValidateProfile(profile, ref strErr_OUT) == true)
                    {
                        Console.WriteLine("Validated parsed file successfully");
                        isSuccess = true;
                    }
                    else
                    {
                        Console.WriteLine("Failed to validate parsed file");
                        isSuccess = false;
                    }
                }
                return isSuccess;
            }
            catch (Exception e)
            {
                Console.WriteLine("seralize_WriteFile ERROR:-" + e.Message);
                isSuccess = false;
                return isSuccess;
            }
        }


        // This Method download the Profiles and extracted it inside Output Directory  
        public static bool fnDownloadPackage(string URL)
        {
            try
            {
                bool isDownloaded = true;
                string parentDirName = new FileInfo(AppDomain.CurrentDomain.BaseDirectory).Directory.Parent.FullName;
                string path = parentDirName + "\\Debug\\";
                if (Directory.Exists(path + "\\package"))
                {
                    Directory.Delete(path + "\\package", true);
                }
                downloadfile(URL, path);
                return isDownloaded;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Inside fnDownloadPackage:Exception: " + ex.Message);
                return false;
            }
        }

        public static void downloadfile(string URL, string path)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var req = client.GetAsync(URL).ContinueWith(res =>
                    {
                        var result = res.Result;
                        if (result.StatusCode == HttpStatusCode.OK)
                        {
                            var readData = result.Content.ReadAsStreamAsync();
                            readData.Wait();

                            var readStream = readData.Result;

                            // Extract filestream and save in Output directory  
                            Stream inStream = readStream;
                            Stream gzipStream = new GZipInputStream(inStream);

                            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                            tarArchive.ExtractContents(path);
                            tarArchive.Close();

                            gzipStream.Close();
                            inStream.Close();
                        }
                    });
                    req.Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        } 

        //This method validates the FHIR resources 
        // Param      
        public static bool ValidateProfile(Base ProfileInstance, ref string strError_OUT)
        {
            bool isValid = true;
            string JsonFileName = "";
            try
            {
                #region Validation
                // The URL profile shall be mentioned below
                string profileURL = "https://nrces.in/ndhm/fhir/r4/package.tgz";
                if (fnDownloadPackage(profileURL) == true)
                {
                    // The path of the extracted profile directory shall be mentioned below
                    string parentDirName = new FileInfo(AppDomain.CurrentDomain.BaseDirectory).Directory.Parent.FullName;
                    string path = parentDirName + "\\Debug\\package\\";
                    string profiledirectory = path;
                    
                    IResourceResolver resolver = new CachedResolver(new MultiResolver(ZipSource.CreateValidationSource(), new DirectorySource(profiledirectory, new DirectorySourceSettings()
                    {
                        IncludeSubDirectories = true,
                    })
                    ));
                    ValidationSettings settings = new ValidationSettings()
                    {
                        ResourceResolver = resolver,
                    };
                    Validator validator = new Validator(settings);
                    var outcome = validator.Validate(ProfileInstance);
                    if (outcome.Success == true)
                    {
                        isValid = true;
                        strError_OUT = "";
                    }
                    else
                    {
                        isValid = false;
                        FhirJsonSerializer serializer = new FhirJsonSerializer(new SerializerSettings()
                        {
                            Pretty = true,
                        });
                        string bundeljsonOutCome = serializer.SerializeToString(outcome);
                        JsonFileName = "Outcome.json";
                        File.WriteAllText(JsonFileName, bundeljsonOutCome);
                        strError_OUT = outcome.ToString();
                    }
                    return isValid;
                }
                else
                {
                    Console.WriteLine("Error while downloading pacakge");
                    return false;
                }
                #endregion


            }
            catch (Exception ex)
            {
                isValid = false;
                strError_OUT = ex.ToString();
                return isValid;
            }
        }
        // Populate Patient Resource
        public static Patient populatePatientResource()
        {
            Patient patient = new Patient()
            {

                Meta = new Meta()
                {
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Patient",
                    },
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0)))
                },
            };
            
            patient.Id = "23986a85-fb64-48c2-ab85-3462586cc134";           

            var id = new Identifier();
            id.System = "https://ndhm.in/SwasthID";
            id.Value = "1234";
            id.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "MR", "Medical record number", "Text");
            patient.Identifier.Add(id);

            var name = new HumanName();
            name.Text = "ABC";
            patient.Name.Add(name);

            patient.Gender = AdministrativeGender.Female;
            patient.BirthDate = "1981-01-12";

            ContactPoint contact1 = new ContactPoint();
            contact1.System = ContactPoint.ContactPointSystem.Phone;
            contact1.Value = "+919818512600";
            contact1.Use = ContactPoint.ContactPointUse.Home;
            patient.Telecom.Add(contact1);

            return patient;
        }

        // Populate Condition Resource
        public static Condition populateConditionResource()
        {
            Condition condition = new Condition()
            {
                
                Meta = new Meta()
                {
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Condition",
                     },
                },

            };

            condition.Id = "825b0dfe-1224-4d2d-8464-0f468a7f693e";
           
            condition.ClinicalStatus = new CodeableConcept("http://terminology.hl7.org/CodeSystem/condition-clinical", "active", "Active", "Active");
            condition.Code = new CodeableConcept("http://snomed.info/sct", "297142003", "Foot swelling", "Foot swelling");

            condition.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");

            return condition;
        }


        // Populate Condition Resource
        public static Condition populateSecondConditionResource()
        {
            Condition condition = new Condition()
            {
                Meta = new Meta()
                {
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Condition",
                     },
                },

            };
            condition.Id = "f9e5c690-3d66-4af8-bc0c-c820a9f5af95";

            

            condition.ClinicalStatus = new CodeableConcept("http://terminology.hl7.org/CodeSystem/condition-clinical", "recurrence", "Recurrence", "Recurrence");

            condition.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            condition.Code = new CodeableConcept("http://snomed.info/sct", "46635009", "Diabetes mellitus type 1", "Diabetes mellitus type 1");

            return condition;
        }

        // Populate Condition Resource
        public static Condition populateThirdConditionResource()
        {
            Condition condition = new Condition()
            {
                Meta = new Meta()
                {
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Condition",
                     },
                },

            };
            condition.Id = "486162b9-7882-43d2-803c-168945920e93";

            
            condition.ClinicalStatus = new CodeableConcept("http://terminology.hl7.org/CodeSystem/condition-clinical", "recurrence", "Recurrence", "Recurrence");
            condition.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            condition.Code = new CodeableConcept("http://snomed.info/sct", "44054006", "Diabetes mellitus type 2", "Diabetes mellitus type 2");

            return condition;
        }

        // Populate Condition Resource
        public static Condition populateFourthConditionResource()
        {
            Condition condition = new Condition()
            {
                Meta = new Meta()
                {
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Condition",
                     },
                },

            };
            condition.Id = "aacc6b0f-771a-4ebf-a19c-5317389a92fd";
           
            condition.ClinicalStatus = new CodeableConcept("http://terminology.hl7.org/CodeSystem/condition-clinical", "recurrence", "Recurrence", "Recurrence");
            condition.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            condition.Code = new CodeableConcept("http://snomed.info/sct", "440700005", "History of aortoiliac atherosclerosis", "Patient complained about pain in left arm");

            return condition;
        }

        // Populate Condition Resource
        public static Condition populateFifthConditionResource()
        {
            Condition condition = new Condition()
            {
                Meta = new Meta()
                {
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Condition",
                     },
                },

            };
            condition.Id = "d0d0a0b1-23a8-4b58-8e58-19dec3a1fc64";
          
            condition.ClinicalStatus = new CodeableConcept("http://terminology.hl7.org/CodeSystem/condition-clinical", "active", "Active", "Active");
            condition.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            condition.Code = new CodeableConcept("http://snomed.info/sct", "22298006", "Myocardial infarction", "pain in the chest, neck, back or arms, as well as fatigue, lightheadedness, abnormal heartbeat and anxiety.");

            return condition;
        }

        public static Composition populatePrescriptionCompositionResource()
        {
            // Set metadata about the resource - Version Id, Lastupdated Date, Profile

            Composition composition = new Composition()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/PrescriptionRecord",
                    },
                },
            };

            // Set logical id of this artifact
            composition.Id = "d687fc03-703f-4a32-9d90-d0691db92898";

            // Set language of the resource content
            composition.Language = "en-IN";          

            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://ndhm.in/phr";
            identifier.Value = "645bb0c3-ff7e-4123-bef5-3852a4784813";

            composition.Identifier = identifier;

            // Status can be preliminary | final | amended | entered-in-error
            composition.Status = CompositionStatus.Final;


            // Kind of composition ("Prescription record ")
            var coding = new List<Coding>();
            coding.Add(new Coding("http://snomed.info/sct", "440545006", "Prescription record"));

            composition.Type = (new CodeableConcept("http://snomed.info/sct", "440545006", "Prescription record"));
            composition.Type.Coding = coding;

            // Set subject - Who and/or what the composition/Prescription record is about
            ResourceReference refrence = new ResourceReference();
            refrence.Reference = "urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134";

            composition.Subject = refrence;

            // Set Timestamp
            composition.DateElement = new FhirDateTime("2017-05-27T11:46:09+05:30");


            // Set author - Who and/or what authored the composition/Presciption record
            composition.Author.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147"));

            // Set a Human Readable name/title
            composition.Title = "Prescription record";


            // Composition is broken into sections / Prescription record contains single section to define the relevant medication requests
            // Entry is a reference to data that supports this section
            ResourceReference reference1 = new ResourceReference();
            reference1.Reference = "urn:uuid:ee2af06a-903d-4387-8ed2-49f89d7da68d";
            reference1.Type = "MedicationRequest";

            ResourceReference reference2 = new ResourceReference();
            reference2.Reference = "urn:uuid:40d49bc0-9ac0-49c7-a3cb-de3da68b431f";
            reference2.Type = "MedicationRequest";

            ResourceReference reference3 = new ResourceReference();
            reference3.Reference = "urn:uuid:859a3e51-5027-486a-bb41-c7773300fd40";
            reference3.Type = "Binary";


            Composition.SectionComponent section = new Composition.SectionComponent();
            section.Title = "Prescription record";
            section.Code = (new CodeableConcept("http://snomed.info/sct", "440545006", "Prescription record"));

            section.Code.Coding = new List<Coding>();
            var codeitem = new Coding();
            codeitem.System = "http://snomed.info/sct";
            codeitem.Code = "440545006";
            codeitem.Display = "Prescription record";

            section.Code.Coding.Add(codeitem);

            section.Entry.Add(reference1);
            section.Entry.Add(reference2);
            section.Entry.Add(reference3);

            composition.Section.Add(section);

            return composition;
        }

        // Populate Practitioner Resource
        public static Practitioner populatePractitionerResource()
        {
            Practitioner practitioner = new Practitioner()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2019, 05, 29, 14, 58, 18, new TimeSpan(1, 0, 0))),

                    Profile = new List<string>()
                    {
                       "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Practitioner",
                    },
                }
            };
            practitioner.Id = "86c1ae40-b60e-49b5-b2f4-a217bcd19147";
 
            var coding = new List<Coding>();
            coding.Add(new Coding("http://terminology.hl7.org/CodeSystem/v2-0203", "MD", "Medical License number"));

            Identifier identifier = new Identifier();
            identifier.System = "https://doctor.ndhm.gov.in";
            identifier.Value = "21-1521-3828-3227";
            identifier.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "MD", "Medical License number");
            identifier.Type.Coding = coding;
            practitioner.Identifier.Add(identifier);

            var name = new HumanName();
            name.Text = "Dr. DEF";
            practitioner.Name.Add(name);
            return practitioner;
        }

        // Populate Second Practitioner Resource
        public static Practitioner populateSecondPractitionerResource()
        {
            Practitioner practitioner = new Practitioner()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2019, 05, 29, 14, 58, 18, new TimeSpan(1, 0, 0))),

                    Profile = new List<string>()
                    {
                       "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Practitioner",
                    },
                }
            };
            practitioner.Id = "aa0f5344-33ca-44a0-b8cf-9aa5b8a227ae";

          
            var coding = new List<Coding>();
            coding.Add(new Coding("http://terminology.hl7.org/CodeSystem/v2-0203", "MD", "Medical License number"));
            Identifier identifier = new Identifier();
            identifier.System = "https://doctor.ndhm.gov.in";
            identifier.Value = "21-1521-3828-3228";
            identifier.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "MD", "Medical License number");
            identifier.Type.Coding = coding;
            practitioner.Identifier.Add(identifier);
            var name = new HumanName();
            name.Text = "Dr. PQR";
            practitioner.Name.Add(name);
            return practitioner;
        }


        // Populate Medication Request Resource
        public static MedicationRequest populateMedicationRequestResource()
        {
            MedicationRequest medicationRequest = new MedicationRequest()
            {

                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/MedicationRequest",
                    },

                },
            };
            medicationRequest.Id = "ee2af06a-903d-4387-8ed2-49f89d7da68d";


            medicationRequest.Status = MedicationRequest.medicationrequestStatus.Active;
            medicationRequest.Intent = MedicationRequest.medicationRequestIntent.Order;

            medicationRequest.Medication = new CodeableConcept("http://snomed.info/sct", "353231006", "Neomycin 5 mg/g cutaneous ointment", "Neomycin 5 mg/g cutaneous ointment");

            medicationRequest.AuthoredOnElement = new FhirDateTime("2020-07-09");

            medicationRequest.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134", "ABC");

            medicationRequest.Requester = new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147", "Dr DEF");


            medicationRequest.ReasonReference.Add(new ResourceReference("urn:uuid:aacc6b0f-771a-4ebf-a19c-5317389a92fd"));

            List<Dosage> dosage1 = new List<Dosage>();
            var objdosage = new Dosage();
            objdosage.Text = "One tablet at once";

            Timing objTiming = new Timing();
            var objTimeRepeat = new Timing.RepeatComponent();
            objTimeRepeat.Frequency = 1;
            objTimeRepeat.Period = 1;
            objTimeRepeat.PeriodUnit = Timing.UnitsOfTime.D;
            objTiming.Repeat = objTimeRepeat;

            objdosage.Timing = objTiming;

            objdosage.AdditionalInstruction.Add(new CodeableConcept("http://snomed.info/sct", "229799001", "Twice a day", "Twice a day"));
            medicationRequest.DosageInstruction = dosage1;
            objdosage.Route = new CodeableConcept("http://snomed.info/sct", "6064005", "Topical route", "Topical route");
            objdosage.Method = new CodeableConcept("http://snomed.info/sct", "421521009", "Swallow", "Swallow");

            dosage1.Add(objdosage);

            medicationRequest.DosageInstruction = dosage1;

            return medicationRequest;
        }


        // Populate Medication Request Resource
        public static MedicationRequest populateSecondMedicationRequestResource()
        {
            MedicationRequest medicationRequest = new MedicationRequest()
            {
                
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),

                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/MedicationRequest",
                    },

                },
            };
            medicationRequest.Id = "40d49bc0-9ac0-49c7-a3cb-de3da68b431f";
            medicationRequest.Status = MedicationRequest.medicationrequestStatus.Active;
            medicationRequest.Intent = MedicationRequest.medicationRequestIntent.Order;

            medicationRequest.Medication = new CodeableConcept("", "", "", "Paracetemol 500mg Oral Tab");

            medicationRequest.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134", "ABC");
            medicationRequest.Requester = new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147", "Dr DEF");

            medicationRequest.ReasonCode.Add(new CodeableConcept("http://snomed.info/sct", "602001", "Ross river fever", "Ross river fever"));

            medicationRequest.ReasonReference.Add(new ResourceReference("urn:uuid:aacc6b0f-771a-4ebf-a19c-5317389a92fd"));

            medicationRequest.AuthoredOnElement = new FhirDateTime("2020-07-09");

            List<Dosage> dosage1 = new List<Dosage>();
            var objdosage = new Dosage();
            objdosage.Text = "Take two tablets orally with or after meal once a day";

            Timing objTiming = new Timing();
            var objTimeRepeat = new Timing.RepeatComponent();
            objTimeRepeat.Frequency = 1;
            objTimeRepeat.Period = 1;

            dosage1.Add(objdosage);

            medicationRequest.DosageInstruction = dosage1;

            return medicationRequest;
        }

        // Populate Binary Resource
        public static Binary populateBinaryResource()
        {
            Binary binary = new Binary()
            {
                
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Binary",
                    },

                },
            };

            binary.Id = "859a3e51-5027-486a-bb41-c7773300fd40";
            binary.ContentType = "application/pdf";
            string author = "R0lGODlhfgCRAPcAAAAAAIAAAACAAICAAAAAgIAA oxrXyMY2uvGNcIyj    HOeoxkXBh44OOZdn8Ggu+DiPjwtJ2CZyUomCTRGO";
            // converts a C# string to a byte array
            byte[] bytes = Encoding.ASCII.GetBytes(author);
            binary.Data = bytes;
            return binary;
        }

        // Populate Composition for DischargeSummary
        public static Composition populateDischargeSummaryCompositionResource()
        {

            // Set logical id of this artifact
            Composition composition = new Composition()
            {
                
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DischargeSummaryRecord",
                    },

                },
            };
            composition.Id = "d687fc03-703f-4a32-9d90-d0691db92898";

            // Set language of the resource content
            composition.Language = "en-IN";        

            // Status can be preliminary | final | amended | entered-in-error
            composition.Status = CompositionStatus.Final;

            // Kind of composition ("Discharge summary")
            //var varcoding = new Coding();
            var coding = new List<Coding>();
            coding.Add(new Coding("http://snomed.info/sct", "373942005", "Discharge summary"));

            composition.Type = (new CodeableConcept("http://snomed.info/sct", "373942005", "Discharge summary", "Discharge summary"));
            composition.Type.Coding = coding;

            // Set subject - Who and/or what the composition/DischargeSummary record is about
            composition.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");

            // Set Context of the Composition
            composition.Encounter = new ResourceReference("urn:uuid:c12a5b45-f88e-4811-be37-9d99658e5bc2");

            // Set Timestamp
            composition.DateElement = new FhirDateTime("2017-05-27T11:46:09+05:30");

            // Set author - Who and/or what authored the composition/DischargeSummary record
            composition.Author.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147"));

            // Set a Human Readable name/title
            composition.Title = "Discharge summary";

            // set custodian 
            composition.Custodian = new ResourceReference("urn:uuid:b9768f37-82cb-471e-934f-71b9ce233656");

            // Set confidentiality as defined by affinity domain
            composition.Confidentiality = Composition.v3_ConfidentialityClassification.N;

            // Composition is broken into sections / DischargeSummary record contains single section to define the relevant medication requests
            // Entry is a reference to data that supports this section

            Composition.SectionComponent section1 = new Composition.SectionComponent();
            section1.Title = "Chief complaints";
            section1.Code = new CodeableConcept("http://snomed.info/sct", "422843007", "Chief complaint section", "Chief complaint section");
            section1.Entry.Add(new ResourceReference("urn:uuid:aacc6b0f-771a-4ebf-a19c-5317389a92fd", "Chief complaint section"));

            Composition.SectionComponent section2 = new Composition.SectionComponent();
            section2.Title = "Medical History";
            section2.Code = new CodeableConcept("http://snomed.info/sct", "1003642006", "Past medical history section", "Past medical history section");
            section2.Entry.Add(new ResourceReference("urn:uuid:8de92e2a-e761-41d5-a44b-152feac98bec"));

            
            Composition.SectionComponent section3 = new Composition.SectionComponent();
            section3.Title = "Investigations";
            section3.Code = (new CodeableConcept("http://snomed.info/sct", "721981007", "Diagnostic studies report", "Diagnostic studies report"));
            section3.Entry.Add(new ResourceReference("urn:uuid:2efefe2d-1998-403e-a8dd-36b93e31d2c8", "Diagnostic studies report"));

            Composition.SectionComponent section4 = new Composition.SectionComponent();
            section4.Title = "Procedures";
            section4.Code = new CodeableConcept("http://snomed.info/sct", "1003640003", "History of past procedure section", "History of past procedure section");
            section4.Entry.Add(new ResourceReference("urn:uuid:57c44e5b-ad77-43e9-9654-27b3b9a4342e"));

            Composition.SectionComponent section5 = new Composition.SectionComponent();
            section5.Title = "Care Plan";
            section5.Code = new CodeableConcept("http://snomed.info/sct", "734163000", "Care plan", "Care plan");
            section5.Entry.Add(new ResourceReference("urn:uuid:ea0a3b8c-ec4a-4ae2-ae86-23191a6c201c"));

            Composition.SectionComponent section6 = new Composition.SectionComponent();
            section6.Title = "Medications";
            section6.Code = new CodeableConcept("http://snomed.info/sct", "1003606003", "Medication history section", "Medication history section");
            section6.Entry.Add(new ResourceReference("urn:uuid:ee2af06a-903d-4387-8ed2-49f89d7da68d"));

            Composition.SectionComponent section7 = new Composition.SectionComponent();
            section7.Title = "Document Reference";
            section7.Code = new CodeableConcept("http://snomed.info/sct", "373942005", "Discharge summary", "Discharge summary");
            section7.Entry.Add(new ResourceReference("urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340"));

            composition.Section.Add(section1);
            composition.Section.Add(section2);
            composition.Section.Add(section3);
            composition.Section.Add(section4);
            composition.Section.Add(section5);
            composition.Section.Add(section6);
            composition.Section.Add(section7);

            return composition;
        }

        // Populate Organization Resource
        public static Organization populateOrganizationResource()
        {
            // Set logical id of this artifact
            Organization organization = new Organization()
            {
                 
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {

                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Organization",
                    },

                },
            };

            organization.Id = "68ff0f24-3698-4877-b0ab-26e046fbec24";
            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://facility.ndhm.gov.in";
            identifier.Value = "4567878";
            identifier.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "PRN", "Provider number", "Provider number");
            organization.Identifier.Add(identifier);

             organization.Name = "UVW Hospital";
            
            List<ContactPoint> list = new List<ContactPoint>();
            ContactPoint contact1 = new ContactPoint();
            contact1.System = ContactPoint.ContactPointSystem.Phone;
            contact1.Value = "+91 24326341234";
            contact1.Use = ContactPoint.ContactPointUse.Work;

            ContactPoint contact2 = new ContactPoint();
            contact2.System = ContactPoint.ContactPointSystem.Email;
            contact2.Value = "contact@facility.uvw.org";
            contact2.Use = ContactPoint.ContactPointUse.Work;

            list.Add(contact1);
            list.Add(contact2);
            organization.Telecom = list;

            return organization;
        }      

        // Populate Organization Resource
        public static Organization populateSecondOrganizationResource()
        {
            // Set logical id of this artifact
            Organization organization = new Organization()
            {
                 
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Organization",
                    },

                },
            };

            organization.Id = "b9768f37-82cb-471e-934f-71b9ce233656";
            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://facility.ndhm.gov.in";
            identifier.Value = "45678781";
            identifier.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "PRN", "Provider number", "null");
            organization.Identifier.Add(identifier);

            organization.Name = "XYZ Lab Pvt.Ltd";

            List<ContactPoint> list = new List<ContactPoint>();
            ContactPoint contact1 = new ContactPoint();
            contact1.System = ContactPoint.ContactPointSystem.Phone;
            contact1.Value = "+91 243 2634 1234";
            contact1.Use = ContactPoint.ContactPointUse.Work;

            ContactPoint contact2 = new ContactPoint();
            contact2.System = ContactPoint.ContactPointSystem.Email;
            contact2.Value = "contact@labs.xyz.org";
            contact2.Use = ContactPoint.ContactPointUse.Work;

            list.Add(contact1);
            list.Add(contact2);
            organization.Telecom = list;

            return organization;

        }

        // Populate Encounter Resource
        public static Encounter populateEncounterResource()
        {
            // Set logical id of this artifact
            Encounter encounter = new Encounter()
            {
                
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 14, 58, 58, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Encounter",
                    },

                },
            };
            encounter.Id = "2ed85a7e-888d-4beb-93df-84e3ccecdb3b";

            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://ndhm.in";
            identifier.Value = "S100";
            encounter.Identifier.Add(identifier);

            encounter.Status = Encounter.EncounterStatus.Finished;
            encounter.Class = new Coding("http://terminology.hl7.org/CodeSystem/v3-ActCode", "AMB", "ambulatory");
            encounter.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            encounter.Period = new Period(new FhirDateTime("2020-04-20T15:32:26.605+05:30"), new FhirDateTime("2020-05-01T15:32:26.605+05:30"));

            Encounter.DiagnosisComponent diagnosiscomponent1 = new Encounter.DiagnosisComponent();
            diagnosiscomponent1.Condition = new ResourceReference("urn:uuid:aacc6b0f-771a-4ebf-a19c-5317389a92fd");
            diagnosiscomponent1.Use = new CodeableConcept("http://snomed.info/sct", "33962009", "Chief complaint", "Chief complaint");
            encounter.Diagnosis.Add(diagnosiscomponent1);


            Encounter.DiagnosisComponent diagnosiscomponent2 = new Encounter.DiagnosisComponent();
            diagnosiscomponent2.Condition = new ResourceReference("urn:uuid:486162b9-7882-43d2-803c-168945920e93");
            diagnosiscomponent2.Use = new CodeableConcept("http://snomed.info/sct", "148006", "Preliminary diagnosis", "Preliminary diagnosis");
            encounter.Diagnosis.Add(diagnosiscomponent2);

            return encounter;
        }


        // Populate Appointment Resource
        public static Appointment populateAppointmentResource()
        {
            // Set logical id of this artifact
            Appointment appointment = new Appointment()
            {
                 
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 14, 58, 58, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Appointment",
                    },

                },
            };

            appointment.Id = "1aeb371d-bb52-469a-9169-55a55aabb4bb";         

            appointment.Status = Appointment.AppointmentStatus.Booked;
            appointment.ServiceCategory.Add(new CodeableConcept("http://snomed.info/sct", "408443003", "General medical practice", "General medical practice"));
            appointment.ServiceType.Add(new CodeableConcept("http://snomed.info/sct", "11429006", "Consultation", "Consultation"));
            appointment.AppointmentType = new CodeableConcept("http://snomed.info/sct", "185389009", "Follow-up visit", "Follow-up visit");

            var appointmentparticipantComponent = new Appointment.ParticipantComponent();
            appointmentparticipantComponent.Actor = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            appointmentparticipantComponent.Status = ParticipationStatus.Accepted;

            var appointmentparticipantComponent1 = new Appointment.ParticipantComponent();
            appointmentparticipantComponent1.Actor = new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147");
            appointmentparticipantComponent1.Status = ParticipationStatus.Accepted;

            appointment.Participant.Add(appointmentparticipantComponent);
            appointment.Participant.Add(appointmentparticipantComponent1);

            appointment.StartElement = new Instant(new DateTimeOffset(2020, 07, 12, 09, 00, 00, new TimeSpan(1, 0, 0)));
            appointment.EndElement = new Instant(new DateTimeOffset(2020, 07, 12, 09, 30, 00, new TimeSpan(1, 0, 0)));
            appointment.Description = "Discussion on the results of your recent Lab Test and further consultation";
            appointment.Created = "2020-07-09T14:58:58.181+05:30";

            appointment.ReasonReference.Add(new ResourceReference("urn:uuid:aacc6b0f-771a-4ebf-a19c-5317389a92fd"));
            appointment.BasedOn.Add(new ResourceReference("urn:uuid:b939ec3c-b18a-4af0-a1a2-200db11ee8cb"));

            return appointment;
        }

        // Populate Encounter Resource
        public static Encounter populateSecondEncounterResource()
        {
            // Set logical id of this artifact
            Encounter encounter = new Encounter()
            {
                  
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 14, 58, 58, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Encounter",
                    },

                },
            };

            encounter.Id = "c12a5b45-f88e-4811-be37-9d99658e5bc2";
            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://ndhm.in";
            identifier.Value = "S100";
            encounter.Identifier.Add(identifier);

            encounter.Status = Encounter.EncounterStatus.Finished;
            encounter.Class = new Coding("http://terminology.hl7.org/CodeSystem/v3-ActCode", "IMP", "inpatient encounter");

            encounter.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            encounter.Period = new Period(new FhirDateTime("2020-04-20T15:32:26.605+05:30"), new FhirDateTime("2020-05-01T15:32:26.605+05:30"));

            var encouterHospitalization = new Encounter.HospitalizationComponent();
            encouterHospitalization.DischargeDisposition = new CodeableConcept("http://terminology.hl7.org/CodeSystem/discharge-disposition", "home", "Home", "Discharged to Home Care");
            encounter.Hospitalization = encouterHospitalization;
            
            return encounter;
        }

        // Populate Diagnostic Report Lab Resource
        public static DiagnosticReport populateDiagonosticReportLabResource()
        {
            // Set logical id of this artifact
            DiagnosticReport diagnosticReportLab = new DiagnosticReport()
            {

                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 58, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DiagnosticReportLab",
                    },

                },
            };
            diagnosticReportLab.Id = "2efefe2d-1998-403e-a8dd-36b93e31d2c8";

            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://xyz.com/lab/reports";
            identifier.Value = "5234342";
            diagnosticReportLab.Identifier.Add(identifier);

            diagnosticReportLab.BasedOn.Add(new ResourceReference("urn:uuid:aa8e4e90-c340-4140-9e12-c0acacc427f6"));
            diagnosticReportLab.Status = DiagnosticReport.DiagnosticReportStatus.Final;
            diagnosticReportLab.Category.Add(new CodeableConcept("http://snomed.info/sct", "708196005", "Hematology service", "Hematology service"));
            diagnosticReportLab.Code = new CodeableConcept("http://loinc.org", "24331-1", "Lipid 1996 panel - Serum or Plasma", "Lipid 1996 panel - Serum or Plasma");
            diagnosticReportLab.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            diagnosticReportLab.Issued = new DateTime(2020, 07, 10, 11, 45, 33);
            diagnosticReportLab.Performer.Add(new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24"));
            diagnosticReportLab.ResultsInterpreter.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147", "Dr.DEF"));
            diagnosticReportLab.Specimen.Add(new ResourceReference("urn:uuid:6fbe092b-d72f-4d71-9ca0-90a3b247fa4c"));

            diagnosticReportLab.Result.Add(new ResourceReference("urn:uuid:81f65384-1005-4605-a276-b274ae006d3b"));
            diagnosticReportLab.Result.Add(new ResourceReference("urn:uuid:aceb6f8a-44de-40f2-9928-bc940b45316e"));
            diagnosticReportLab.Result.Add(new ResourceReference("urn:uuid:e64c7482-bde6-4b1f-95bc-2f23bf2ee333"));
            diagnosticReportLab.Conclusion = "Elevated cholesterol/high density lipoprotein ratio";

            return diagnosticReportLab;
        }

        // Populate Observation/Cholesterol Resource
        public static Observation populateCholesterolObservationResource()
        {
            // Set logical id of this artifact
            Observation observation = new Observation()
            {
                
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "2",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 58, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Observation",
                    },

                },
            };

            observation.Id = "81f65384-1005-4605-a276-b274ae006d3b";
            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "2093-3", "Cholesterol [Mass/volume] in Serum or Plasma", "Cholesterol [Mass/volume] in Serum or Plasma");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Performer.Add(new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24"));

            var quantity = new Quantity(Convert.ToDecimal("156"), "mg/dL", "http://snomed.info/sct");
            quantity.Code = "258797006";
            observation.Value = quantity;

            Observation.ReferenceRangeComponent obserrefrange = new Observation.ReferenceRangeComponent();
            var quantity1 = new Quantity(200, "mg/dL", "http://snomed.info/sct");
            quantity1.Code = "258797006";
            obserrefrange.Low = quantity1;

            return observation;
        }

        // Populate Observation/Triglyceride Resource
        public static Observation populateTriglycerideObservationResource()
        {
            Observation observation = new Observation()
            {
                
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "3",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 58, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Observation",
                    },

                },
            };

            observation.Id = "aceb6f8a-44de-40f2-9928-bc940b45316e";
            observation.Status = ObservationStatus.Final;
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Performer.Add(new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24"));

            observation.Code = new CodeableConcept("http://loinc.org", "35217-9", "Triglyceride [Mass or Moles/volume] in Serum or Plasma", "Triglycerides, serum by Enzymatic method");

            var quantity = new Quantity(Convert.ToDecimal("146"), "mg/dL", "http://snomed.info/sct");
            quantity.Code = "258797006";
            observation.Value = quantity;

            Observation.ReferenceRangeComponent obserrefrange = new Observation.ReferenceRangeComponent();
            var quantity1 = new Quantity(150, "mg/dL", "http://snomed.info/sct");
            quantity1.Code = "258797006";
            obserrefrange.Low = quantity1;

            return observation;
        }

        // Populate Observation/Cholesterol In HDL Resource
        public static Observation populateCholesterolInHDLObservationResource()
        {
            Observation observation = new Observation()
            {
                 
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "3",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 58, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Observation",
                    },

                },
            };

            observation.Id = "e64c7482-bde6-4b1f-95bc-2f23bf2ee333";
            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "2085-9", "Cholesterol in HDL [Mass/volume] in Serum or Plasma", "Cholesterol in HDL [Mass/volume] in Serum or Plasma");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Performer.Add(new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24"));

            var quantity = new Quantity(Convert.ToDecimal("45"), "mg/dL", "http://snomed.info/sct");
            quantity.Code = "258797006";
            observation.Value = quantity;

            Observation.ReferenceRangeComponent obserrefrange = new Observation.ReferenceRangeComponent();
            var quantity1 = new Quantity(40, "mg/dL", "http://snomed.info/sct");
            quantity1.Code = "258797006";
            obserrefrange.High = quantity1;

            return observation;
        }

        // Populate Procedure Resource
        public static Procedure populateProcedureResource()
        {
            Procedure procedure = new Procedure()
            {
                
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 58, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Procedure",
                    },

                },
            };

            procedure.Id = "8de92e2a-e761-41d5-a44b-152feac98bec";
            procedure.Status = EventStatus.Completed;
            procedure.Code = new CodeableConcept("http://snomed.info/sct", "36969009", "Placement of stent in coronary artery", "Placement of stent in coronary artery");

            procedure.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            procedure.Performed = new FhirDateTime("2019-05-12");

            procedure.FollowUp.Add((new CodeableConcept("http://snomed.info/sct", "394725008", "Diabetes medication review", "Diabetes medication review")));
            return procedure;
        }

        // Populate Procedure Resource
        public static Procedure populateSecondProcedureResource()
        {
            Procedure procedure = new Procedure()
            {
               
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 58, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Procedure",
                    },

                },
            };

            procedure.Id = "57c44e5b-ad77-43e9-9654-27b3b9a4342e";
            procedure.Status = EventStatus.Completed;

            procedure.Code = new CodeableConcept("http://snomed.info/sct", "232717009", "Coronary artery bypass grafting", "Coronary artery bypass grafting");
            procedure.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");

            return procedure;
        }

        // Populate Care Plan Resource
        public static CarePlan populateCarePlanResource()
        {
            CarePlan carePlan = new CarePlan()
            {
                Id = "CarePlan-01",
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 58, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/CarePlan",
                    },

                },
            };

            carePlan.Status = RequestStatus.Active;
            carePlan.Intent = CarePlan.CarePlanIntent.Plan;
            carePlan.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134", "ABC");

            return carePlan;
        }

        // Populate Care Plan Resource
        public static CarePlan populateSecondCarePlanResource()
        {
            CarePlan carePlan = new CarePlan()
            {
                 
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 58, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/CarePlan",
                    },

                },
            };
            carePlan.Id = "ea0a3b8c-ec4a-4ae2-ae86-23191a6c201c";
            carePlan.Status = RequestStatus.Active;
            carePlan.Intent = CarePlan.CarePlanIntent.Plan;
            carePlan.Category.Add(new CodeableConcept("http://snomed.info/sct", "736368003", "Coronary heart disease care plan", "Coronary heart disease care plan"));
            carePlan.Title = "Coronary heart disease care plan";
            carePlan.Description = "Treatment of coronary artery and related disease problems";

            carePlan.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134", "ABC");

            CarePlan.ActivityComponent activitycomponent = new CarePlan.ActivityComponent();
            activitycomponent.OutcomeReference.Add(new ResourceReference("urn:uuid:1aeb371d-bb52-469a-9169-55a55aabb4bb"));

            carePlan.Activity.Add(activitycomponent);
            return carePlan;
        }

        // Populate Composition for OPConsultNote
        public static Composition populateOPConsultNoteCompositionResource()
        {
            // Set metadata about the resource - Version Id, Lastupdated Date, Profile
            Composition composition = new Composition()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/OPConsultRecord",
                    },
                },
            };

            // Set logical id of this artifact
            composition.Id = "5927729b-0b79-462e-903f-62b4b5b2edef";

            // Set language of the resource content
            composition.Language = "en-IN";             

            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://ndhm.in/phr";
            identifier.Value = "645bb0c3-ff7e-4123-bef5-3852a4784813";

            composition.Identifier = identifier;

            // Status can be preliminary | final | amended | entered-in-error
            composition.Status = CompositionStatus.Final;

            // Kind of composition ("Prescription record ")
            var coding = new List<Coding>();
            coding.Add(new Coding("http://snomed.info/sct", "371530004", "Clinical consultation report"));

            composition.Type = (new CodeableConcept("http://snomed.info/sct", "371530004", "Clinical consultation report"));
            composition.Type.Coding = coding;

            // Set subject - Who and/or what the composition/Prescription record is about
            ResourceReference refrence = new ResourceReference();
            refrence.Reference = "urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134";
            refrence.Display = "ABC";
            composition.Subject = refrence;

            // Set Context of the Composition
            composition.Encounter = new ResourceReference("urn:uuid:2ed85a7e-888d-4beb-93df-84e3ccecdb3b");

            // Set Timestamp
            composition.DateElement = new FhirDateTime("2017-05-27T11:46:09+05:30");

            // Set author - Who and/or what authored the composition/Presciption record
            composition.Author.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147", "Dr DEF"));

            // Set a Human Readable name/title
            composition.Title = "Consultation Report";

            // Set Custodian - Organization which maintains the composition            
            composition.Custodian = new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24", "UVW Hospital");

            // Composition is broken into sections / OPConsultNote record contains single section to define the relevant medication requests
            // Entry is a reference to data that supports this section
            Composition.SectionComponent section1 = new Composition.SectionComponent();
            section1.Title = "Chief complaints";
            section1.Code = new CodeableConcept("http://snomed.info/sct", "422843007", "Chief complaint section", "Chief complaint section");
            section1.Entry.Add(new ResourceReference("urn:uuid:825b0dfe-1224-4d2d-8464-0f468a7f693e"));

            Composition.SectionComponent section2 = new Composition.SectionComponent();
            section2.Title = "Allergies";
            section2.Code = new CodeableConcept("http://snomed.info/sct", "722446000", "Allergy record", "Allergy record");
            section2.Entry.Add(new ResourceReference("urn:uuid:c80baf7f-1cea-4dcb-9f94-1cad8d157fce"));

            Composition.SectionComponent section3 = new Composition.SectionComponent();
            section3.Title = "Medical History";
            section3.Code = new CodeableConcept("http://snomed.info/sct", "371529009", "History and physical report", "History and physical report");
            section3.Entry.Add(new ResourceReference("urn:uuid:f9e5c690-3d66-4af8-bc0c-c820a9f5af95"));

            Composition.SectionComponent section4 = new Composition.SectionComponent();
            section4.Title = "Investigation Advice";
            section4.Code = new CodeableConcept("http://snomed.info/sct", "721963009", "Order document", "Order document");
            section4.Entry.Add(new ResourceReference("urn:uuid:b939ec3c-b18a-4af0-a1a2-200db11ee8cb"));

            Composition.SectionComponent section5 = new Composition.SectionComponent();
            section5.Title = "Medications";
            section5.Code = new CodeableConcept("http://snomed.info/sct", "721912009", "Medication summary document", "Medication summary document");
            section5.Entry.Add(new ResourceReference("urn:uuid:64df5cfe-16f1-4532-b596-803dd72f47fa"));
            section5.Entry.Add(new ResourceReference("urn:uuid:ee2af06a-903d-4387-8ed2-49f89d7da68d"));

            Composition.SectionComponent section6 = new Composition.SectionComponent();
            section6.Title = "Procedure";
            section6.Code = new CodeableConcept("http://snomed.info/sct", "371525003", "Clinical procedure report", "Clinical procedure report");
            section6.Entry.Add(new ResourceReference("urn:uuid:8de92e2a-e761-41d5-a44b-152feac98bec"));

            Composition.SectionComponent section7 = new Composition.SectionComponent();
            section7.Title = "Follow Up";
            section7.Code = new CodeableConcept("http://snomed.info/sct", "736271009", "Outpatient care plan", "Outpatient care plan");
            section7.Entry.Add(new ResourceReference("urn:uuid:1aeb371d-bb52-469a-9169-55a55aabb4bb"));

            Composition.SectionComponent section8 = new Composition.SectionComponent();
            section8.Title = "Document Reference";
            section8.Code = new CodeableConcept("http://snomed.info/sct", "371530004", "Clinical consultation report", "Clinical consultation report");
            section8.Entry.Add(new ResourceReference("urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340"));

            composition.Section.Add(section1);
            composition.Section.Add(section2);
            composition.Section.Add(section3);
            composition.Section.Add(section4);
            composition.Section.Add(section5);
            composition.Section.Add(section6);
            composition.Section.Add(section7);             

            return composition;
        }

        // Populate Allergy Intolerance Resource
        public static AllergyIntolerance populateAllergyIntoleranceResource()
        {
            AllergyIntolerance allergyIntolerance = new AllergyIntolerance()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/AllergyIntolerance",
                    },
                },
            };
            // Set logical id of this artifact
            allergyIntolerance.Id = "c80baf7f-1cea-4dcb-9f94-1cad8d157fce";

            allergyIntolerance.Code = new CodeableConcept("http://snomed.info/sct", "716186003", "No known allergy", "NKA");
            allergyIntolerance.VerificationStatus = new CodeableConcept("http://terminology.hl7.org/CodeSystem/allergyintolerance-verification", "confirmed", "Confirmed", "Confirmed");

            allergyIntolerance.Patient = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            allergyIntolerance.ClinicalStatus = new CodeableConcept("http://terminology.hl7.org/CodeSystem/allergyintolerance-clinical", "active", "Active", "Active");

            allergyIntolerance.Recorder = new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147");
            Annotation annotation = new Annotation();
            Markdown markdown = new Markdown("The patient reports no other known allergy.");
            annotation.Text = markdown;
            allergyIntolerance.Note.Add(annotation);

            return allergyIntolerance;
        }

        // Populate Service Request Resource
        public static ServiceRequest populateServiceRequestResource()
        {
            ServiceRequest serviceRequest = new ServiceRequest()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ServiceRequest",
                    },
                },
            };
            serviceRequest.Id = "b939ec3c-b18a-4af0-a1a2-200db11ee8cb";
             

            serviceRequest.Status = RequestStatus.Active;
            serviceRequest.Intent = RequestIntent.OriginalOrder;

            serviceRequest.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            serviceRequest.Occurrence = new FhirDateTime("2020-07-09T15:32:26.181+05:30");
            serviceRequest.Requester = new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147", "Dr PQR");
            return serviceRequest;
        }

        // Populate Service Request Resource
        public static ServiceRequest populateServiceRequestResourceForLab()
        {
            ServiceRequest serviceRequest = new ServiceRequest()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ServiceRequest",
                    },
                },
            };
            serviceRequest.Id = "aa8e4e90-c340-4140-9e12-c0acacc427f6";
            
            serviceRequest.Status = RequestStatus.Active;
            serviceRequest.Intent = RequestIntent.OriginalOrder;
            serviceRequest.Code = new CodeableConcept("http://snomed.info/sct", "16254007", "Lipid Panel", "Lipid Panel");
            serviceRequest.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            serviceRequest.Occurrence = new FhirDateTime("2020-07-08T09:32:26.181+05:30");
            serviceRequest.Requester = new ResourceReference("urn:uuid:aa0f5344-33ca-44a0-b8cf-9aa5b8a227ae", "Dr PQR");
            return serviceRequest;
        }
        // Populate Medication Statement Resource
        public static MedicationStatement populateMedicationStatementResource()
        {
            MedicationStatement medicationStatement = new MedicationStatement()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/MedicationStatement",
                    },
                },
            };
            medicationStatement.Id = "64df5cfe-16f1-4532-b596-803dd72f47fa";
            

            medicationStatement.Status = MedicationStatement.MedicationStatusCodes.Completed;
            medicationStatement.Medication = new CodeableConcept("http://snomed.info/sct", "134463001", "Telmisartan 20 mg oral tablet", "Telmisartan 20 mg oral tablet");
            medicationStatement.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            medicationStatement.DateAssertedElement = new FhirDateTime("2020-02-02T14:58:58.181+05:30");
            return medicationStatement;
        }

        // Populate Document Reference Resource
        public static DocumentReference populateDocumentReferenceResource()
        {
            DocumentReference documentReference = new DocumentReference()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DocumentReference",
                    },
                },
            };
            documentReference.Id = "ff92b549-f754-4e3c-aef2-b403c99f6340";
            

            documentReference.Status = DocumentReferenceStatus.Current;
            documentReference.DocStatus = CompositionStatus.Final;
            documentReference.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            documentReference.Type = new CodeableConcept("http://snomed.info/sct", "4241000179101", "Laboratory report", "Laboratory report");
            var DocumentReferenceContentComponent = new DocumentReference.ContentComponent();
            DocumentReferenceContentComponent.Attachment = new Attachment();
            DocumentReferenceContentComponent.Attachment.ContentType = "application/pdf";
            DocumentReferenceContentComponent.Attachment.Language = "en-IN";
            DocumentReferenceContentComponent.Attachment.Title = "Laboratory report";
            DocumentReferenceContentComponent.Attachment.CreationElement = new FhirDateTime("2019-05-29T14:58:58.181+05:30");


            byte[] bytes = Encoding.Unicode.GetBytes("d2SXEWqsucNWTk1qqZdpF8qK1XOln6IkbwKT1tpx7VtN3UmtnyGVnazdujjChApk6KZWotVpNqjKxMCzGqMNcrpNLxN+WSNcoNHvBCHMSDD/ygQxNoCs1knWoO/SVPXQ1PyQr1NDwDz8Jz8Dy8AC/CS/AyzMCHayRTrEY3NDM5QTA0Qjk5MDQ4RUU2M0VFPl0gL1ByZXYgMTg0MTE0L1hSZWZTdG0gMTgzODMwPj4NCnN0YXJ0eHJlZg0KMTg0NzMxDQolJUVPRg==");
            DocumentReferenceContentComponent.Attachment.Data =  bytes;
            documentReference.Content.Add(DocumentReferenceContentComponent);
 


            return documentReference;
        }

        public static Composition populateHealthDocumentRecordCompositionResource()
        {
            Composition composition = new Composition()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/HealthDocumentRecord",
                    },
                },
            };

            // Set logical id of this artifact
            composition.Id = "ecae12dd-9966-41f0-b44b-8d3eabf14111";

            // Set language of the resource content
            composition.Language = "en-IN";

            

            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://ndhm.in/phr";
            identifier.Value = "645bb0c3-ff7e-4123-bef5-3852a4784813";

            // Status can be preliminary | final | amended | entered-in-error
            composition.Status = CompositionStatus.Final;

            // Kind of composition ("HealthDocument record")
            composition.Type = new CodeableConcept("http://snomed.info/sct", "419891008", "Record artifact", "Record artifact");

            // Set subject - Who and/or what the composition/HealthDocument record is about
            composition.Subject = (new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134", "ABC"));

            // Set Timestamp
            composition.DateElement = new FhirDateTime("2020-07-09T15:32:26.605+05:30");

            // Set author - Who and/or what authored the composition/HealthDocument record
            composition.Author.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147", "Dr DEF"));

            // Set a Human Readable name/title
            composition.Title = "Health Document";


            ResourceReference reference3 = new ResourceReference();
            reference3.Reference = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";

            Composition.SectionComponent section = new Composition.SectionComponent();
            section.Title = "HeathDocument record";
            section.Entry.Add(new ResourceReference("urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340"));
            composition.Section.Add(section);

            return composition;

        }
        // Populate Composition for DiagnosticReport
        public static Composition populateDiagnosticReportRecordDCMCompositionResource()
        {
            Composition composition = new Composition()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DiagnosticReportRecord",
                    },
                },
            };

            // Set logical id of this artifact
            composition.Id = "d687fc03-703f-4a32-9d90-d0691db92898";

            // Set language of the resource content
            composition.Language = "en-IN";           

            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://ndhm.in/phr";
            identifier.Value = "645bb0c3-ff7e-4123-bef5-3852a4784813";

            // Status can be preliminary | final | amended | entered-in-error
            composition.Status = CompositionStatus.Final;

            // Kind of composition ("Diagnostic studies report")
            composition.Type = new CodeableConcept("http://snomed.info/sct", "721981007", "Diagnostic studies report", "null");

            // Set subject - Who and/or what the composition/DiagnosticReport record is about
            composition.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");

            // Set Timestamp
            composition.DateElement = new FhirDateTime("2017-05-27T11:46:09+05:30");

            // Set author - Who and/or what authored the composition/DiagnosticReport record
            composition.Author.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147"));

            // Set a Human Readable name/title
            composition.Title = "Diagnostic Report- Imaging DICOM";

            // Composition is broken into sections / DiagnosticReport record contains single section to define the relevant medication requests
            // Entry is a reference to data that supports this section
            ResourceReference reference1 = new ResourceReference();
            reference1.Reference = "urn:uuid:2efefe2d-1998-403e-a8dd-36b93e31d2c8";
            reference1.Type = "DiagnosticReport";

            ResourceReference reference2 = new ResourceReference();
            reference2.Reference = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";
            reference2.Type = "DocumentReference";

            Composition.SectionComponent section = new Composition.SectionComponent();
            section.Title = "Computed tomography imaging report";
            section.Code = new CodeableConcept("http://snomed.info/sct", "4261000179100", "Computed tomography imaging report", "null");
            section.Entry.Add(reference1);
            section.Entry.Add(reference2);

            composition.Section.Add(section);

            return composition;
        }

        // Populate Diagnostic Report Imaging DCM Resource
        public static DiagnosticReport populateDiagnosticReportImagingDCMResource()
        {
            DiagnosticReport diagnosticReportImaging = new DiagnosticReport()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DiagnosticReportImaging",
                    },
                },
            };
            diagnosticReportImaging.Id = "2efefe2d-1998-403e-a8dd-36b93e31d2c8";

          

            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://xyz.com/lab/reports";
            identifier.Value = "5234342";
            diagnosticReportImaging.Identifier.Add(identifier);

            diagnosticReportImaging.BasedOn.Add(new ResourceReference("urn:uuid:b939ec3c-b18a-4af0-a1a2-200db11ee8cb"));

            diagnosticReportImaging.Status = DiagnosticReport.DiagnosticReportStatus.Final;
            diagnosticReportImaging.Category.Add(new CodeableConcept("http://snomed.info/sct", "310128004", "Computerized tomography service", "Computerized tomography service"));
            diagnosticReportImaging.Code = new CodeableConcept("http://loinc.org", "82692-5", "CT Head and Neck WO contrast", "CT Head and Neck WO contrast");
            diagnosticReportImaging.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            diagnosticReportImaging.Issued = new DateTime(2020, 07, 10, 11, 45, 33);
            diagnosticReportImaging.Performer.Add(new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24", "XYZ Lab Pvt.Ltd."));
            diagnosticReportImaging.ResultsInterpreter.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147", "Dr. DEF"));
            diagnosticReportImaging.ImagingStudy.Add(new ResourceReference("urn:uuid:29530ff8-baa7-4669-9afb-0b37fb4c6982", "HEAD and NECK CT DICOM imaging study"));


            var link = new DiagnosticReport.MediaComponent();
            link.Link = new ResourceReference("urn:uuid:35e0e4fa-1d49-4aa4-bd82-5ae9338e8703");
            diagnosticReportImaging.Media.Add(link);

            diagnosticReportImaging.Conclusion = "CT brains: large tumor sphenoid/clivus.";
            return diagnosticReportImaging;
        }

        // Populate Diagnostic Report Media Resource
        public static DiagnosticReport populateDiagnosticReportMediaResource()
        {
            DiagnosticReport diagnosticReportmedia = new DiagnosticReport()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DiagnosticReportImaging",
                    },
                },
            };
            diagnosticReportmedia.Id = "c62f6355-2d37-40dc-80d9-cdc5efe531be";

          

            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://xyz.com/lab/reports";
            identifier.Value = "5234342";
            diagnosticReportmedia.Identifier.Add(identifier);

            diagnosticReportmedia.BasedOn.Add(new ResourceReference("urn:uuid:b939ec3c-b18a-4af0-a1a2-200db11ee8cb"));

            diagnosticReportmedia.Status = DiagnosticReport.DiagnosticReportStatus.Final;
            diagnosticReportmedia.Category.Add(new CodeableConcept("http://snomed.info/sct", "310128004", "Computerized tomography service", "Computerized tomography service"));
            diagnosticReportmedia.Code = new CodeableConcept("http://loinc.org", "82692-5", "CT Head and Neck WO contrast", "CT Head and Neck WO contrast");
            diagnosticReportmedia.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            diagnosticReportmedia.Issued = new DateTime(2020, 07, 10, 11, 45, 33);
            diagnosticReportmedia.Performer.Add(new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24", "XYZ Lab Pvt.Ltd."));
            diagnosticReportmedia.ResultsInterpreter.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147", "Dr. DEF"));



            var link = new DiagnosticReport.MediaComponent();
            link.Link = new ResourceReference("urn:uuid:35e0e4fa-1d49-4aa4-bd82-5ae9338e8703");
            diagnosticReportmedia.Media.Add(link);

            diagnosticReportmedia.Conclusion = "CT brains: large tumor sphenoid/clivus.";
            diagnosticReportmedia.ConclusionCode.Add(new CodeableConcept("http://snomed.info/sct", "188340000", "Malignant tumor of craniopharyngeal duct", "Malignant tumor of craniopharyngeal duct"));

            var attachment = new Attachment();
            attachment.ContentType = "application/pdf";
            attachment.Language = "en-IN";
            byte[] bytes = Encoding.ASCII.GetBytes("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MDktMDJUMDQ6NTA6NDgrMDA6MDA8L3htcDpNb2RpZnlEYXRlPjwvcmRmOkRlc2NyaXB0aW9uPgo8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiAgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iPgo8eG1wTU06RG9jdW1lbnRJRD51dWlkOjA4Q0VCODFELTA0QjQtNEM1Qi05Q0RCLUEwMEM4MjFBQkY4ODwveG1wTU06RG9jdW1lbnRJRD48eG1wTU06SW5zdGFuY2VJRD51dWlkOjA4Q0VCODFELTA0QjQtNEM1Qi05Q0RCLUEwMEM4MjFBQkY4ODwveG1wTU06SW5zdGFuY2VJRD48L3JkZjpEZXNjcmlwdGlvbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCjwvcmRmOlJERj48L3g6eG1wbWV0YT48P3hwYWNrZXQgZW5kPSJ3Ij8+DQplbmRzdHJlYW0NCmVuZG9iag0KNDggMCBvYmoNCjw8L0Rpc3BsYXlEb2NUaXRsZSB0cnVlPj4NCmVuZG9iag0KNDkgMCBvYmoNCjw8L1R5cGUvWFJlZi9TaXplIDQ5L1dbIDEgNCAyXSAvUm9vdCAxIDAgUi9JbmZvIDQ0IDAgUi9JRFs8MURCOENFMDhCNDA0NUI0QzlDREJBMDBDODIxQUJGODg+PDFEQjhDRTA4QjQwNDVCNEM5Q0RCQTAwQzgyMUFCRjg4Pl0gL0ZpbHRlci9GbGF0ZURlY29kZS9MZW5ndGggMTUxPj4NCnN0cmVhbQ0KeJw1z0sOAWEMwPF+4/0cgxmPiY2tGSFIcAALiY27iIQziFs4hMO4yuj0Txf9pUnbtCIaWeY0ByI5L/gY3sSY3YxkaqR0pge4G/OrsTgZy6OxCo21bvF0ZyQDGMIIxhDBrzPWuc37X3XAgQcFKEIJylCBKtSgDg1oQgva4EMAPehCH0K9bHu2x3awf+Q4/2LET5Evs5QQXg0KZW5kc3RyZWFtDQplbmRvYmoNCnhyZWYNCjAgNTANCjAwMDAwMDAwMTUgNjU1MzUgZg0KMDAwMDAwMDAxNyAwMDAwMCBuDQowMDAwMDAwMTY2IDAwMDAwIG4NCjAwMDAwMDAyMjIgMDAwMDAgbg0KMDAwMDAwMDU0MSAwMDAwMCBuDQowMDAwMDExMTMwIDAwMDAwIG4NCjAwMDAwMTEyOTggMDAwMDAgbg0KMDAwMDAxMTUzNyAwMDAwMCBuDQowMDAwMDExNTkwIDAwMDAwIG4NCjAwMDAwMTE2NDMgMDAwMDAgbg0KMDAwMDAxMTg5NSAwMDAwMCBuDQowMDAwMDEyMzY1IDAwMDAwIG4NCjAwMDAwMTI2MTggMDAwMDAgbg0KMDAwMDAxMzA3NyAwMDAwMCBuDQowMDAwMDEzMzQxIDAwMDAwIG4NCjAwMDAwMDAwMTYgNjU1MzUgZg0KMDAwMDAwMDAxNyA2NTUzNSBmDQowMDAwMDAwMDE4IDY1NTM1IGYNCjAwMDAwMDAwMTkgNjU1MzUgZg0KMDAwMDAwMDAyMCA2NTUzNSBmDQowMDAwMDAwMDIxIDY1NTM1IGYNCjAwMDAwMDAwMjIgNjU1MzUgZg0KMDAwMDAwMDAyMyA2NTUzNSBmDQowMDAwMDAwMDI0IDY1NTM1IGYNCjAwMDAwMDAwMjUgNjU1MzUgZg0KMDAwMDAwMDAyNiA2NTUzNSBmDQowMDAwMDAwMDI3IDY1NTM1IGYNCjAwMDAwMDAwMjggNjU1MzUgZg0KMDAwMDAwMDAyOSA2NTUzNSBmDQowMDAwMDAwMDMwIDY1NTM1IGYNCjAwMDAwMDAwMzEgNjU1MzUgZg0KMDAwMDAwMDAzMiA2NTUzNSBmDQowMDAwMDAwMDMzIDY1NTM1IGYNCjAwMDAwMDAwMzQgNjU1MzUgZg0KMDAwMDAwMDAzNSA2NTUzNSBmDQowMDAwMDAwMDM2IDY1NTM1IGYNCjAwMDAwMDAwMzcgNjU1MzUgZg0KMDAwMDAwMDAzOCA2NTUzNSBmDQowMDAwMDAwMDM5IDY1NTM1IGYNCjAwMDAwMDAwNDAgNjU1MzUgZg0KMDAwMDAwMDA0MSA2NTUzNSBmDQowMDAwMDAwMDQyIDY1NTM1IGYNCjAwMDAwMDAwNDMgNjU1MzUgZg0KMDAwMDAwMDAwMCA2NTUzNSBmDQowMDAwMDE0NDE4IDAwMDAwIG4NCjAwMDAwMTQ2NzQgMDAwMDAgbg0KMDAwMDAxNDk4OSAwMDAwMCBuDQowMDAwMDY5NDkzIDAwMDAwIG4NCjAwMDAwNzI1OTEgMDAwMDAgbg0KMDAwMDA3MjYzNiAwMDAwMCBuDQp0cmFpbGVyDQo8PC9TaXplIDUwL1Jvb3QgMSAwIFIvSW5mbyA0NCAwIFIvSURbPDFEQjhDRTA4QjQwNDVCNEM5Q0RCQTAwQzgyMUFCRjg4PjwxREI4Q0UwOEI0MDQ1QjRDOUNEQkEwMEM4MjFBQkY4OD5dID4+DQpzdGFydHhyZWYNCjcyOTg4DQolJUVPRg0KeHJlZg0KMCAwDQp0cmFpbGVyDQo8PC9TaXplIDUwL1Jvb3QgMSAwIFIvSW5mbyA0NCAwIFIvSURbPDFEQjhDRTA4QjQwNDVCNEM5Q0RCQTAwQzgyMUFCRjg4PjwxREI4Q0UwOEI0MDQ1QjRDOUNEQkEwMEM4MjFBQkY4OD5dIC9QcmV2IDcyOTg4L1hSZWZTdG0gNzI2MzY+Pg0Kc3RhcnR4cmVmDQo3NDE0NQ0KJSVFT0Y=");

            attachment.DataElement = new Hl7.Fhir.Model.Base64Binary(bytes);
            attachment.Title = "Diagnostic Report";
            diagnosticReportmedia.PresentedForm.Add(attachment);

            return diagnosticReportmedia;
        }
        // Populate Imaging Study Resource
        public static ImagingStudy populateImagingStudyResource()
        {
            ImagingStudy imagingStudy = new ImagingStudy()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ImagingStudy",
                    },
                },
            };
            imagingStudy.Id = "29530ff8-baa7-4669-9afb-0b37fb4c6982";
         

            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://xyz.in/DCMServer";
            identifier.Value = "7897";
            imagingStudy.Identifier.Add(identifier);

            imagingStudy.Status = ImagingStudy.ImagingStudyStatus.Available;
            imagingStudy.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            imagingStudy.Interpreter.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147"));
            imagingStudy.NumberOfSeries = 1;
            imagingStudy.NumberOfInstances = 1;

            ImagingStudy.SeriesComponent img = new ImagingStudy.SeriesComponent();
            img.Uid = "2.16.124.113543.6003.2588828330.45298.17418.2723805630";
            img.Number = 1;
            img.Modality = new Coding("http://snomed.info/sct", "429858000", "CT of head and neck");
            img.Description = "CT Surview 180";
            img.NumberOfInstances = 1;
            img.BodySite = new Coding("http://snomed.info/sct", "774007", "Structure of head and/or neck");

            ImagingStudy.InstanceComponent instancecomponent = new ImagingStudy.InstanceComponent();
            instancecomponent.Uid = "2.16.124.113543.6003.189642796.63084.16748.2599092903";
            instancecomponent.SopClass = new Coding("urn:ietf:rfc:3986", "urn:oid:1.2.840.10008.5.1.4.1.1.2");
            instancecomponent.Number = 1;
            instancecomponent.Title = "CT of head and neck";
            img.Instance.Add(instancecomponent);
            imagingStudy.Series.Add(img);

            return imagingStudy;
        }
        // Populate Media Resource
        public static Media populateMediaResource()
        {
            Media media = new Media()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Media",
                    },
                },
            };
            media.Id = "35e0e4fa-1d49-4aa4-bd82-5ae9338e8703";
          
            media.Status = EventStatus.Completed;

            var attachment = new Attachment();
            attachment.ContentType = "image/jpeg";
            byte[] bytes = Encoding.ASCII.GetBytes("/9j/4AAQSkZJRgABAQEASABIAAD/4RBGaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA0LjIuMi1jMDYzIDUzLjM1MjYyNCwgMjAwOC8wNy8zMC0xODowNTo0MSAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iIHhtbG5zOnBob3Rvc2hvcD0iaHR0cDovL25zLmFkb2JlLmNvbS9waG90b3Nob3AvMS4wLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0RXZ0PSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VFdmVudCMiIHhtbG5zOnRpZmY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vdGlmZi8xLjAvIiB4bWxuczpleGlmPSJodHRwOi8vbnMuYWRvYmUuY29tL2V4aWYvMS4wLyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ1M0IE1hY2ludG9zaCIgeG1wOkNyZWF0ZURhdGU9IjIwMTAtMDktMTZUMTI6MDg6MjArMTA6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDEyLTAyLTIyVDIxOjQ3OjUzKzExOjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDEyLTAyLTIyVDIxOjQ3OjUzKzExOjAwIiBkYzpmb3JtYXQ9ImltYWdlL2pwZWciIHBob3Rvc2hvcDpDb2xvck1vZGU9IjMiIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6QzA5MTI1MTMwODIwNjgxMThGNjJFN0NBOEIzRUI0RDYiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6QzA5MTI1MTMwODIwNjgxMThGNjJFN0NBOEIzRUI0RDYiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDpDMDkxMjUxMzA4MjA2ODExOEY2MkU3Q0E4QjNFQjRENiIgdGlmZjpPcmllbnRhdGlvbj0iMSIgdGlmZjpYUmVzb2x1dGlvbj0iNzIwMDAwLzEwMDAwIiB0aWZmOllSZXNvbHV0aW9uPSI3MjAwMDAvMTAwMDAiIHRpZmY6UmVzb2x1dGlvblVuaXQ9IjIiIHRpZmY6TmF0aXZlRGlnZXN0PSIyNTYsMjU3LDI1OCwyNTksMjYyLDI3NCwyNzcsMjg0LDUzMCw1MzEsMjgyLDI4MywyOTYsMzAxLDMxOCwzMTksNTI5LDUzMiwzMDYsMjcwLDI3MSwyNzIsMzA1LDMxNSwzMzQzMjs3NEE0OEU0MzU4NDYyNEQyMDI3NzZBRkNGOUU5MTFGQyIgZXhpZjpQaXhlbFhEaW1lbnNpb249Ijc2NiIgZXhpZjpQaXhlbFlEaW1lbnNpb249Ijc3MCIgZXhpZjpDb2xvclNwYWNlPSI2NTUzNSIgZXhpZjpOYXRpdmVEaWdlc3Q9IjM2ODY0LDQwOTYwLDQwOTYxLDM3MTIxLDM3MTIyLDQwOTYyLDQwOTYzLDM3NTEwLDQwOTY0LDM2ODY3LDM2ODY4LDMzNDM0LDMzNDM3LDM0ODUwLDM0ODUyLDM0ODU1LDM0ODU2LDM3Mzc3LDM3Mzc4LDM3Mzc5LDM3MzgwLDM3MzgxLDM3MzgyLDM3MzgzLDM3Mzg0LDM3Mzg1LDM3Mzg2LDM3Mzk2LDQxNDgzLDQxNDg0LDQxNDg2LDQxNDg3LDQxNDg4LDQxNDkyLDQxNDkzLDQxNDk1LDQxNzI4LDQxNzI5LDQxNzMwLDQxOTg1LDQxOTg2LDQxOTg3LDQxOTg4LDQxOTg5LDQxOTkwLDQxOTkxLDQxOTkyLDQxOTkzLDQxOTk0LDQxOTk1LDQxOTk2LDQyMDE2LDAsMiw0LDUsNiw3LDgsOSwxMCwxMSwxMiwxMywxNCwxNSwxNiwxNywxOCwyMCwyMiwyMywyNCwyNSwyNiwyNywyOCwzMDtFNzUyQ0Q2NzMyRDk1MEQ4MTg5N0QyMDYxOEE4MUZGRSI+IDx4bXBNTTpIaXN0b3J5PiA8cmRmOlNlcT4gPHJkZjpsaSBzdEV2dDphY3Rpb249ImNyZWF0ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6QzA5MTI1MTMwODIwNjgxMThGNjJFN0NBOEIzRUI0RDYiIHN0RXZ0OndoZW49IjIwMTItMDItMjJUMjE6NDc6NTMrMTE6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDUzQgTWFjaW50b3NoIi8+IDwvcmRmOlNlcT4gPC94bXBNTTpIaXN0b3J5PiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8P3hwYWNrZXQgZW5kPSJ3Ij8+/+EZEEV4aWYAAE1NACoAAAAIAAcBEgADAAAAAQABAAABGgAFAAAAAQAAAGIBGwAFAAAAAQAAAGoBKAADAAAAAQACAAABMQACAAAAHgAAAHIBMgACAAAAFAAAAJCHaQAEAAAAAQAAAKQAAADQAAr8gAAAJxAACvyAAAAnEEFkb2JlIFBob3Rvc2hvcCBDUzQgTWFjaW50b3NoADIwMTI6MDI6MjIgMjE6NDc6NTMAAAOgAQADAAAAAf//AACgAgAEAAAAAQAAAv6gAwAEAAAAAQAAAwIAAAAAAAAABgEDAAMAAAABAAYAAAEaAAUAAAABAAABHgEbAAUAAAABAAABJgEoAAMAAAABAAIAAAIBAAQAAAABAAABLgICAAQAAAABAAAX2gAAAAAAAABIAAAAAQAAAEgAAAAB/9j/4AAQSkZJRgABAgAASABIAAD/7QAMQWRvYmVfQ00AAv/uAA5BZG9iZQBkgAAAAAH/2wCEAAwICAgJCAwJCQwRCwoLERUPDAwPFRgTExUTExgRDAwMDAwMEQwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwBDQsLDQ4NEA4OEBQODg4UFA4ODg4UEQwMDAwMEREMDAwMDAwRDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDP/AABEIAKAAnwMBIgACEQEDEQH/3QAEAAr/xAE/AAABBQEBAQEBAQAAAAAAAAADAAECBAUGBwgJCgsBAAEFAQEBAQEBAAAAAAAAAAEAAgMEBQYHCAkKCxAAAQQBAwIEAgUHBggFAwwzAQACEQMEIRIxBUFRYRMicYEyBhSRobFCIyQVUsFiMzRygtFDByWSU/Dh8WNzNRaisoMmRJNUZEXCo3Q2F9JV4mXys4TD03Xj80YnlKSFtJXE1OT0pbXF1eX1VmZ2hpamtsbW5vY3R1dnd4eXp7fH1+f3EQACAgECBAQDBAUGBwcGBTUBAAIRAyExEgRBUWFxIhMFMoGRFKGxQiPBUtHwMyRi4XKCkkNTFWNzNPElBhaisoMHJjXC0kSTVKMXZEVVNnRl4vKzhMPTdePzRpSkhbSVxNTk9KW1xdXl9VZmdoaWprbG1ub2JzdHV2d3h5ent8f/2gAMAwEAAhEDEQA/APKkkkklKSSSSUpJJaXQ/q/1Xr2X9l6bSbHNg22n211t/wBJfafbWz/pv/waSnNRsfEyspxZjU2XuHLa2l5/6AK9Rw/qP9UfqviV9Q+sNreo3Ogt3u9LF3fS20sn1czb/wBcrsr/AMBWh5v+NmrCrGN0XCroqBIpY1gY3b+ZuqAb6f8AmpKeAr+q31mt/m+k5ro8Mez/AMgr1P8Ai9+ulzQ5nSbgD++WMP8Am2vY5bnUv8Zn1voyrMZ97KLmkAsYxjwD9Jm1w3Nd9JZnU/rt9c97LH9QuaxwDq7GwG6jt6Y2b27vz/0jElNa3/F79c6vpdKtJiYaWOMD+TW9yz8j6t/WLGn7R0zLqA5LqLAP87ZtVmr66/W2oks6rkkuO4y8u14/O3LTr/xlfW/Et2/bzkRtJ3gETH0OPzHfT/0iSnk3scxxa9pa4ctIgj71Fej4f+MvpvVm/ZvrT03HymuIaL3VgloMh7+N9ez96r3p+sf4uul9ToGb9U7tlzgT+zrn7mvIG4sw8x3tc7+Ra/8ASfznqVpKfN0kS+i/GufRkVupuqcW2VvBa5rh9Jr2O9zXIaSlJJJJKUkkkkp//9DypJJJJSkklsfVb6uZP1i6qzCqd6VDB6mXkH6NVLfpv/lP/MqZ/pP+D96Smz9UPqdm/WTJc7d9m6ZjGczNd9Fo+l6dc/Tuc3/tv/CLvczq2B0jp9uD0CqrH6d09227Jvk0NyDHvyQ39J1bq+xu6rBZ+hwv0duX+lprowrX1hycHp3SLui9PecPp/TaC/KbSQHVt9rKq7H6+r1HNyMin9H/AIH1PtF/6b0Vw+RZm9SZhXXVMo6bhhv2PptQ9jKz/h7v9PlXfzt1jvp/+BJKWycrL6q+2x1e85A3P6hlEvy3NM+1u39Fj1u+lXj4/wDNsVSj6uUiwWW3PLpBbsMGfFzjuctk1+md41YRqPA+Kn6e8At4/wBYSU0T0+luOWU1gWAyHj6YdP8AObz7nO/lpn9PxwHPtaHW2vL3O7lzjuc7+0tJtIGvA7lDvb6hho1Alo7pKcrpGHVTQLAA97yZf3gHbs1H5u3a7+WjdR6bRllosbG3hzTDoPi+He3clgDdU7aZPqWajgy97tP85aO0OYC4T3MchJTzbfqy4OBNwf8AyCCAf7bZ/wCoRukdT679Xc706Htdj2OAuxrz+rvE+1x3H9F6c/0iv07MdbjWNnceBqPL+SgGgXttD2NeH6EOEgj6JSU9N1PofTfrvhWbm/s76yYbvRcbo3bo/QY+a5n9Ipua39Vz2f8AqO7yfMw8rByrcPLrdTkUOLLa3chwXa9KuzcXKrxDfvvros/Zl9gOraR69vSM7Zt9XCyKa/0Hv9XEyPRuxvSW/wDW76u1fW3ow61g0vr63iVn1aDBffVXpbQ9lf8AOZuF/N7q/wCc+h6X6fE9JKfJUkkklKSSSSU//9HypJJJJS7Wlzg1oJcTAA1JJXsHTKcf6l9Ho6Uxod1jIFWX1NzYa8Bx9uM20eoz9AzfVT79nrfrf6P1Vxv+LXpVWR1a/rOUJxOh1fajpM36/Yq9oLX7t7X3V7f8JT6f563+s3WX5V77bHnIrNdN24e05RH+UGscHFrasV/6Kr0/9Fs/wVdlqU5v1ivqyq8To2JWaGXPddmOa/e0sr9zaq9A/wBP1Hb/ANI6z9L6f6SyxExWWgBjzuaO/eAqGCbb+qX3WNhtTfSY0cNBPqRuHt9/tctNxP0W6HkR/BJS9psYJABYOWgT939VCdX+dU72kglv4uVlo9vYR4KNdZZI1IMwPL/zFJS8Szb4D8VVsY8S8EtIlzSO3b/qXbVfA7FDtZ7NBr/rokpz6aBXQxrJIA1Py8FbrbDRrMwRKekN9NjToQ57fiJa5qK6BE6mNe6SkFjNC1vc6lQsa4VhrDEaE91YIdoYiQAQdVFwLRuPzCSnKysa6xzLG27Hs1peTDWPH0HmPpMs/m7t3+Ds3rqfq19aq6fRyLQaXMaaerY5Y4kZNH6H7ZTt3O9b7H6bL2f9qqqfp/aKK/XxHMrs18tW9iCgPsGP1Cu0kh+TXG4HX1sVzNtzt37+JZ6X/HpKYf4zvqxX0vqjOr4DQel9W/TMdXrWy13vtra/Vvp3T9oo/wCuMqZsoXFL1zptNXV+l5X1YznB2Fkvczp+UQC2jKA9bG9Ph1VWQz9JXX/x+JX/ADq8py8TIwsq7DyWenkY73VWsMGHsOx7Zb7fpBJSFJJJJT//0vKkklOqs22sqby9waPiTCSn1r6r9KPTfqx0zFdUHuzN/Ws0+o1rdtLfV6fVd/hfsu5uLbdu/Rep6lf+EXP5NtLOlnKybXuD3Mroc1pix+RYftuU9/t9rKa8j09n+FfX/oV6D1cWYvTupMxmV1VYmAMBltgB0bW2173sG6x7GV5NNWPVVv8AUyfU9b9GvOPrk5tteLVSx7aRtqxWPgPBax5sLWV/oq6335H81V/ISUiwLLsqt11Feyt1j3NAJj931H/S9Vzm+1v+Dor/AJpWKsq37QaTS428ua2Axo4L9/u/nP3Vbx8aqmllFTS2tg2saTJHxd+8ge9nUDUG7g5m4vnWA6Gj+wkpsA2uO4kDWBE9vNH078H8EqxPiBJ0Pl/K/lJyDJ09wBB7JKW3gEE6ACf9ih+ls3FrXv2gl21pOg0/NCJVV6riXO9Otur3nkD81rP5SmdrWOyWBzyP0dbTI/lF/td9H/q0lOdisvFj7rj9IgNZGgDfz/7TnKy54c+dOJ+fmoMy91wOSSanfTIgbRI921v7iV1NtGQ/Hsjex0Eg+14P0LGfybGpKSNAJjvxGsSVXcbPULfpDl06c/mt/NVlo08+/n8f5SVjdNRr4pKaTb6mkteRW9muxxExPLW/ScxVcw1W5GJYxpt+zXAlrB7j6g2P2fv2bm1vrr/PR76W25lTWyHBrnPLeQxu36Th+ZucqeThESGB73mdtoBa1oaWua79/dv/AMJ/g0lOv0vqVWI8X3FzOn5rPQy3sBd7WOJqzsfn9P0bJfXbv+nQz1v8J6Sp/wCMzA+0HD+sjGgWZQOH1RrNQzNxx6Vg5/wrGfodv+Cx/W/wqD0jJy7bepdFyYe0XhxZXEeuN+M66r+TkOFXrbP5xdFkYtvV/q1m4Zf9oyOpdPx+r07GaNycZox8yp0fRyshmN6Pt+nbVekp8pSSSSU//9PypEosNd9dg5Y9rh8jKGi41fq5FVX+ke1v3nakp9x+sthNXVqHPk5dmPVjOrcRsbdXTZkMuZH06qunW5Pt+nRauErczO69bkBs09Ob6FA5Hrv/AJ23X/Rf+fF2P1vzDg32viW05D8g+EV4WPS0f+D3riuh/ounVbiHW3D1nzyS8udKSnWayHiOB4+CrW1OdmMtrkek0m0RwyQyr/OutR6rA5xiQRyP9qayp/r+oONsTzyZ1+5JSSCGiACRxJ0nzSZDjAHtGh8SpFpLTt1PI+/sq9dm1wkwZjwSU28Rjchxx7LPTD7BJ5hoP0v5W1vvXRj6p4dVD3UXm4WMkVvJJaRHurbV/Obv3FzmOW2X0Au2OdaxrrO4JO31G/yl6A/GYwNDmDVwl2p+ieHbUlPn2X0WwOFoYaarrPTYHAg6x7tvv/eRPrC2hvVa6sclzaqm1lx5lvtl3+Ytz629X+y31YjN32oM9Rh4a31CWfpP3vY32rlWD3mx3vce51n98uSUl3GJ5jWR5p3D2guJg6STP9ZDeQZk+4wJ40RTqwHiNYSU0N7XXinUCN+kAOJ0Ywu+k/btc9T27HbmuLbPzbB9IfByZ2M2y4WECahoT23mNzP+Eb+/+4+1EcPd2jvCSnEpptwfrE124vdnUufW8aH1Gnd+b+f6uP8A9NeifVu3D+0Mulxfjfbn0MGjRvNWbU7Q+6v0My1te/8APXA/WKasfE6jUT6mFkAgg/muixv/AE6l2v1dZXYa7BummvMpGwwHB1VV+L64/Pr9Cy7Z/Lpq/cSU+O5Gw5FprEM3u2gdhPtQ0kklP//U8qRsNzW5lDn/AEW2MLvgHBBTgwQfAykp9Y/xlWvFea9pcan1i2s8R6rMXGeJ13N2VrJwfSHTsKlkaY1JIgTue31N5eR+89XPrvluz/q3h9RAmvOwGbSf38e1os/tWMyd/wD1pVsWPsuM6mPT+zY4aO4Iqrb/ANJ25JSSJDZJ9pmJ7/RkR7URji9hHYfjCEdAAdAfD8qnXtDdNAIA8p/dSUy5A117EeH8pQtrEN26Hg/NMy3c2xjh7gQTrpALmz+CcP2vawODIHtJ+if5L/zm/wBdqSmfbaPLjy+jBW5/z3zcTDdOMzIyY2tvLoZ/Xuq/f/e2Lm7MhlTtt49FzSNJlkHixr2/4P8AltRMhrTRYIJlsgDvp7UlMWB1o9a9xe9wGp147Dd+Yz6KKSHNLBxr5fkUaG/q7Z9oDAXbuBpqnpi2v12f0ckAWPMbiPzam/Stc38530GJKYVsI3a69vDTzUwXHUjgd/NElrjHB7xAEKhkdUx2+maPew7pedAdsNe797b6n0ElNgMAExwh2OA5P+v5oRg8OYHdnAEIQcwat0d5c6/9JJTS6tRv6ZlCxupYHtZ3DWObfY937u5rNjFvfUJ/rNxdxPvocHToCBTdV/1FbFj50NwMt3b0LY76+m9av+L+Kelv6g8/o8LDvIceC9zvRq/s7GW/56Sny142vc3wJCiiX2C2+ywCA9znAeAJlDSU/wD/1fKkkkklPo31Zaev/UDN6VIOR0x/2irncayHMya//YV7/T/4XYqvRDYej1h7ttuJY7FuaIMbZsofP7ljPVYz/iFU/wAV3Wx0v6xsrsdFOUPTf/r/AOCf9aW51fpNH1a+sl/TQ2OmdWDbae3pte/0/TY/877Bl+i/b/3ByL0lLBzOTBj8VN23buHfvzoqwrDCa3zuaS0jwI9v/VI7HAAQOfuSUhuG3e5oBe271GiNXVWMi9m7870cpvrM/cRay148R/r2KZzWuG06jWE0Aaj4keaSkOW2s49gc0OYBujw/lMcPorKpyc4sbj1OdNhayndpve52zbV/WWy+z0qzYW7oncwjnxbtVNlNVsNuq3uYXOAn3h0h72fytr/ANIz/rjElNK7Jyg52NY8WGl3ur5EgB22xzI3+k8+nbX/AKZb5E7GvGtdba9deGy52nt/SWuc9mxZoox67mVVs2WPlz28uAB9z7Ofda5y0CSYgwW+ehH0tp/s/QSU0eq5zcSj0wJfboGjjz3uWRVaxvTMuy/6ZArxzuEuc5wdZWK/pe1rt/qLXz8f1tYnxCpYPSGfaG33A+nUdzaz+c7lo/qfnOSU7NVbq6mVP0cytrHT+8Bqmc0ESTBiNO8KRJALidzjqfiUMvgT2/10SU1OsZNeN0vKduhz6zUPEmz9GWgf1Xe5X7bT0X/Fxc1xLLs8txmTy4VS/Jsb/UzL7qP+tb1lZtdvVOq9P6Hi177rrRY5sEt0BcwP/wCDax3r5H/AJf4zs2lufjdFxSfs3TKxUAeS5ssda6P8JZZ6u9JTxKSSSSn/1vKkkkklJ8HJOJmU5Ov6J4cQOSJ9w/zV699acGn6xfU2rr2PU27qXTttj3VgTbW0BuSwu+l6T8Z32lm36C8aXo3+LH65V9Od+zcx59M6NESS3luz+XVud7f9EkpQzBlllxEjIqbYyyNHmIuez+X6jf0rf9IpAgNk6l2gidJ+CtfWX6t5H1bsv6j06s5v1cvm9jaiN2HdO9m2N2/AfZ/o/oUf8Kyu7IoYWU3JoGQze2wwL2Aj2P8Ap1xt27qb6v01FjfZ/PJKbLQ7hwhzdCD2I0UeCfNJh8IAHCTuZA4SUqHCB+byP9f6yp52O4gWVtl4+kJj2iT9JXT9Gex/ihXbxW+Glx2ugDTtxqkpD0+l9bTY+sVutDXDWTDhuhxVknXQa8fJOXh3Gu6Dp30UASRr8yUlK7yT5+XzUmmIJ5URr5pnu7eGqSmZd9yiz7M13qZUnFpBtyADB9NnudWP5V3tpZ/xiWo2ggjeWtYIJLnPO2uqpn+Ettd9Ctv/ABn81vT09Gyev9Qb0DBc41DbZ1bLEBlNRJcyndq12S9vsqq/0nrP/mWetUlOr9S6304nUfr91RoOXnbq8FhEMa2W1+xu7+bdYz0K/bvpw8T/AIVeXdXzjn9SyMsncLXktPEj807fzd30l6J/jO69i42HR0PppDaKGCilrTIDGt9J7v8Atr9Wr/68vL0lKSSSSU//1/KkkkklKTtc5rg5pIcDII0IITJJKfRfqJ/jFtw7GdP6gQWPIAeTDXHxM/zV/wDL/mrv8L7/ANKtT6xfVodNe/6w/Vyn7X0m3TNwKRFlIJ3vfj1x/MMf+l+z7f1N/wDN/qfqeh5Muv8Aqp/jAz+i2NryHOtpEAP1cYH5l7NzfXrbPs/w1X/gaSnYoupzMb7TiP8AWoOm9vIP7l1f+CtTh8tnTVblvS/qd9am/tLCvPROpXD3ZeK7bVY53ud6w/R12fpP5z1PsuTZZ/OLL6l0D6zdGqc7Kw/2jQ0e3MwPfI/NdkYfttr9vvsfR+gSU1wYkk6ASfgUNjLHHbS02ud7aqR3efaxjP3d/wBBAb1Dp1jWtZlUnt7nioyP3m3ek9qNjO6a/Jqty+rY+NjVHe5jLWue867W/oSdrGu970lJr8R3TnjBdc3JfTW2LW6F7D7Zez8zbc22v/hKmMvr/R2qAMjXWNCPj2S6pf0J+Q2+jPxb6nM2NHqBrme42/Qlv093/opU3dR6bU3XLpDW9mvDz/m1eo9ySm2X6aST4KFuRTXS65xOxn0iBuj/AF/lIvT8DrPWGB3Ren2X1uJBy8n9BjCC5j3B1hbdk+nY3bZ6CuDovRemNZl/WfqVfU3UGR07EivCa6XaZF42/af6jv0v+DsqurSU0uj9E699YrftVbmdM6Kxrw7qbzPt0be3GFmzfY7+Z+01/qvpsvr9d/8ANq51r61dE+q/S/2F9Xg4tJLrrCf017z9K3ItAa5lb/6vqXM/RUspx1g/Wf8Axi5vUx9lwYpxWQ2trW7WMDfa30af6v8Ahrf+tV0LjHOc9xe8lznGXOOpJPclJSbMzMjNyHZGS/fY/wC4Ds1o/Na1ASSSUpJJJJT/AP/Q8qSSSSUpJJJJSkkkklJ8TNy8Oz1MW51Lu+0wD/Wb9F/9pdX0X/GZ1npzRW8lzB+5G0/GiwOq/wC2PQXGpJKfS3f4yOg9Rd6nWOlYeXbt2i2/H9w/dl/65/0VNv13+pNfvq6H01jjMH0ZI/8AZVq8xSSU+nZP17+qBLdnRummRqTjh/8A6IrQW/4zem4IJ6X03DxHagOx8YNI00/PoXm6SSnretf4x+t9T3MDy2t2m1x9v/bFeyn/ALdbauYyczKy378m11rhoNx0A8GN+iz+ygpJKUkkkkpSSSSSlJJJJKf/2f/bAEMACAYGBwYFCAcHBwkJCAoMFA0MCwsMGRITDxQdGh8eHRocHCAkLicgIiwjHBwoNyksMDE0NDQfJzk9ODI8LjM0Mv/bAEMBCQkJDAsMGA0NGDIhHCEyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMv/AABEIAboBtwMBIgACEQEDEQH/xAAfAAABBQEBAQEBAQAAAAAAAAAAAQIDBAUGBwgJCgv/xAC1EAACAQMDAgQDBQUEBAAAAX0BAgMABBEFEiExQQYTUWEHInEUMoGRoQgjQrHBFVLR8CQzYnKCCQoWFxgZGiUmJygpKjQ1Njc4OTpDREVGR0hJSlNUVVZXWFlaY2RlZmdoaWpzdHV2d3h5eoOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4eLj5OXm5+jp6vHy8/T19vf4+fr/xAAfAQADAQEBAQEBAQEBAAAAAAAAAQIDBAUGBwgJCgv/xAC1EQACAQIEBAMEBwUEBAABAncAAQIDEQQFITEGEkFRB2FxEyIygQgUQpGhscEJIzNS8BVictEKFiQ04SXxFxgZGiYnKCkqNTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqCg4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2dri4+Tl5ufo6ery8/T19vf4+fr/2gAMAwEAAhEDEQA/APn+iiigAooooAKKKKAClpKWgAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAopdrH+E/lS7HHVG/KgBtFLtb+6fypCMdaACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKAEooooAKKKWgBKKWigBKKWigBKWiigAooooAKKKKACiiigAooooAKKK1tE8Nat4hulg06zklLHG4LwKAMmrVnpt7fuEtbaSUnj5Vr3jwp+z8IzHc6/cBj1MK161p/hnw94agUwWcMYXgMwBNAHy9o/wAKPEeqMCbZokPciuxsvgFfSLmeRgfrivebrxBY2dvvjKkA4wOMVj3fj+yt5DjbsA4yetAHmsP7PcaxlpJ8t6bquwfs+6eIx5s43ema6K7+KltBI4DKcj5fY1hf8LieMsjsCfXFAFpPgFogIzL9eK27L4N+GLXG+38zHtXFP8ZrmJ2jjO7d69qzbv4w6q67I2KY6NmgD1g/Dfwjapvlso1Ud2IAqb/hAfCjqJvsEPlkcZ4FfPV98UNQuJAbq4eTac7c8VnXvxV1+6wn2hhEvCqDjFAH0e/w/wDCNwjxw2dv5mOApBwazbX4ZeF9Qt2FxYok6EqwGOK+dYviHrVvKJobqRZOvWtFPixrhlEjytu7kHrQB7Re/A7w5P8A6jCk9qwrr9n6yckQXHT0NcbafFvVFAdnYg8Zz0rVsfi7eI5Mkzc980ASXnwBuEX9zM249Oa5vUfgtr1kCUUv+Fd7Y/GCZ5t8sq+UnQetdVYfFSyvHUvs2AcigD5n1HwhremMfPspMDqVFYrIyHDqVI7EYr7YtdW0LW48yQwsCP4lBrC134Y+GPEEReCKKJz3ToaAPkKivWvFXwT1PSzJLp581ByF9q8tvLG5sJ2huoWikHGGFAFeiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooASiiigApaSloAKKKKACiiigAooooAKKKKACiiigAooooAKns7O4v7lLe1heWVzgKoya2/CXgzVfGGpJa2ELbM/PKR8qivpfwr4E8PfDjTRd3WyW8PBlYZJPoo9aAPPPAvwGmuFjv/Ej+UnDLbL1P1r2q1ttE8L2iwWNrHEAMARr8x/GoDe3WqQNcTv9hsl5wThmHvXC+IPiTYaXL5VjF9quE4QDn8aAOt1XxJeQqdoWEHoD1xXnfiLx7HEpg+1eY49D0NcTr+v+INbd7q4ZoUPYcYrhpJxEXkkk3vnuc0Adtf8Aja6iQu0xYN/DmuVuPFF1fyld7Afw81gT3Lztlj1p1koa6TcwVQeTQB0N1DfR2Udw6uFP8Z6VmS36kjfIWYelbup699rs49NiHnRqMAqKw/7Cn27ijKKAI1voWnDNuGeOKs6hHKtqGjVyjfxGp9G0Fbq98mYhCRlS1WzqE9rcvp08KyxRnHAoA5I5zz1pK3721ik3MsWwHoMVlT2MkQVgMq3SgCrRUnlMH2Yy5/hHNbNn4clnjBlbbnnFAFXSYC9wRL/qdp3Z6VSnkPnOqn5QSBiunHhx1hZEkIzWRqOhT2ID4LL3NAGYJZF4DsB9atwarcwKAjEevNUaKAO48N+OrnTZ2EkjbCMKCa9I8M+Pf7Sheykm8ucncjZ618/1YgvZ7aVJInKuhyCKAPsPSvFUVwEW8wcLtbvzWf4q8AaF40tHe2WNLjHUDFeG6F4+CyQtdEiReGHZq9l8P6xDqEIvNLugJSMlM0AfPvjHwFqvhK7ZbiFmgzw4HSuTr7QnfTvE9m2navbqkpG0Eivnv4j/AAtu/DF3JdWMbS2R5+UdKAPM6KKKACiiigAooooAKKKKACiiigAooooAKKKKAEooooAKWkpaACiiigAooooAKKKKACiiigAooooAK7b4f/DvUPGuophGjsEYebKRxj2qf4cfDa+8bX4lZTFp0TDzJSOG9hX1BFHpXgrQ4rS1iVVUYjjX70hoAis7DRfAegpBawqoA2qqj55WrntV1S00vOveJZ0a5xm0sQeEHbI9a53xP44TTJzPIRdas4xFCOVhH0rjk0a91rzNd8SXRVB8wRjQBPq3jPWPGFy9vbM1vak/ORxgVkRXuleH7g+YRPKP4zzWRrfiyCL/AELSIgo6Fh3rOTTpJbZPOJaWQ5JPagDV1nxBb6q5jiYqhHIUVyh0z9/uIYx9Tmuss9JhgUHaM+4q2tsrHlBj6UAcPHos08h2KQueBWrbeFG2Zlbj0rrVtolwQAKf1+X0oAyLHTYLT7iDjvitTyxsy4H0odfLxxnJ6VNt8wHtgUAc7dQt9tDxkrjpipobJXl3kZY9SavfZd8pZuBVpIAMEUAVWs4WQqyA8Vzut2CJYuUz8hyB6V1UpEYwDzWNqRXyC7DKA/MPagDI8PacpT7Q65Y9M11KRDaCDg+lVdPjh8hXgGImHAq8QPlx2oAmEaMuT1FUrsK0RjZd27tV+NSyZziopYlB9x0oA5KXw5FO0jiTy2HasOfS54Zdigv9K9B8lQxJHWkW0iVslQTQB50bC5AyYmA+lW4LKFIS8jsJuy44rvPs6kElBj6VE2nW7qcxKSfagDzmcPvyy4x3Famh+JdR0G5WW1mbAOSua09V0p4I2khUOv8AEprD/s93Ie3G4ddjdaAPevC/xCs/ElukdyBHcAfe6YNemaXdwataNpuoosyMMKzDIYV8x+GvD095G9xZyeVcR8tFnk16J4K8YyWd9/ZeqZXcdqu38JoAyPip8I5tIeTV9GjMloeZI1HK14wQQcEYIr7jsr+K5H9nXxSRpF+UnkSL/jXg3xf+FbaVM+t6NCTaNzLGo+6aAPFaKKKACiiigAooooAKKKKACiiigAooooASiiigApaSloAKKKKACiiigAooooAKKKKACvQfhn8Nbvxtqayzq0WlxHMsuPvewrG8C+Dbzxnr8VlAhECsDNJjhVr67s7Ky8K6HBp+nwACNQkcajl29TQAsUWl+EdDS3tokhghXCRr1Y14/wCN/GN0ZjPbYkuZPkQdox7V13i6drG3Nxqc+65cfJEDwPbFeb39lbWsVve3km0s3mEH0oA1PCvglLKL/hIdem3s37w+Ya4L4ieNf7b1B7HTiUs0O0Be9Q+LPH2q+KJhYWhZLOP5VRO49TWLbaObTbJMN0p5A9KAL+kaF5UCzSrl2GRW6igIocBXWrFlhoIs9MU+6tw5zjkdKABV3Y9qeqZf2FMgfgZ6jrVlTkH1NAEJGM8U0bkOT+FTsoRCOuajWNsDd+VAEYyfmPWpY+uc8d6eIxjJp4j9KAEbBPQHNPKjysd6VU+XJpZyqqAvJNAGZcKC+PSqN1Gr2Nwh5+XNajgM2SMVnXoYRzlRxtxQBS0mUtpkK9w2K2N+1QKyNFX/AEFRjo1azJ8w2jPrQBZiO4c9qVo9/PemQnB21ZwF4oApledvcd6FAUc8VcKK/wAw4xUUiBhgdaAIHXIBzxUkQ30qJhDkZ9qlIEacDmgDOvgCpwPbFUG0dLlllDeWwHatC4OSvqTUwX5M4oA5MXmo+FNfivFkZ0z+DD0ru5JdN8U2YuLN1S6YZwOCGrn7+y/tCExyDg9K5drTUvD92Lm1kbCHOR/WgD2Hwr4tkMo0PWGMV3Af3E5OM/jXr2k6hHrlhLYajEpl2lXU9HX1FeBQJB418MLe2/7vVLflyvXIrrfAfiubU7P7Fcv5WrWDYRjx5ijtQB578VvhxN4S1Rr20Vn06diykD7ntXmlfcV7Y2Hi/wAPPa3SK0cq4Yd0avkjx74MuvBuvSWkik27HMT44IoA5SiiigAooooAKKKKACiiigAooooASiiigApaSloAKKKKACiiigAooooAKt6bp1zq2owWNpGZJpmCqAKqV9IfA74fLplh/wAJLqkWLiZf3CuPuL60Adp4H8K2fw+8MRxlN97MAZCB8zN6Voavr1joFrJqV9IGumX5I8/d9AKj1LWl+07tmZWykAPb3rzbU7S6bUpTqTm4YNlIxzzQBPYsfFHii1u9TkLRu2/y+wWvMfilr/2/xZc2lmdttCfLVRXoOpXy+HNNl1ALtuDGQFP8NeI2jSajrLXMvzEuZHJoA67w/pEdnpwkZMzSckmtWWxSROnIFFjcCSJTgADtV8EMCfWgDMsonTKNwAeK1AodMEc+tCw55IwalyEHtQBnyQlCWHekilw+DwRV5ijnB5/pVaaABSe/Y0ATKFkUc80vlgP8wz71TjmKnDDBHSpfO3uArfWgCwAdwGMipPKGS36UyLJGQcnNTs3IwOtAEEjBFx69qhwQnB6dDUjKuTuqs7YcoKABD5jjI61R1QG2tpNwOWHAqWcuY90Zwy81W1e7+1pbED5wMOKAKGk52AAkZPIrbUYGAKp2MHlKCV61cLZ60ASom1l5+tWvvZB7dDVJHIOGPB6VaibPA5xQASL8oqJCUbkZz3qztLufQVFIoIGeAKAD3HNNkkzxjmpEAUenpVaYlWLDvQBX2tJIUA59auRp+7xikhjUYY9TUshCHGcUAQtENvTmqd3biWIqQCcVYa46qTiqt3eLDC2OWI4oAz/AGoyaP4wewY4huCVIPSuy/ssQ6xdajATH5L5JHevL75ri2vbfVoUIEbDLD1r1Twdrdrqepg3LA2d4m2Rc/db1oA9H8Nan5UA1OOQvbsAJ0HOPer3j/wAH2fjfwzIqqDchN8Eg6+uK5PR4x4J8TSWVzcLNpV591m6YNem2KpZIsccge0k5ibP3c9qAPh7UdPn0u/msrlCksTFWBFVa+hPjz4D8yIeJbCL5l4uFUfrXz3QAUUUUAFFFFABRRRQAUUUUAJRRRQAUtJS0AFFFFABRRRQAUUU6ONpZFjQFnY4AHc0Adz8KfBp8X+LokmU/YrbEsx9cdBX1Fr2owaZYpaRFY1CgED+FB2rnvhd4Rh8F+DknuUC3k6ebOx6juBXF+K9Wv7zW3u0fbZg7dpP3qAJJvEU664L64ANsnEUY70tn4jkS5uXe2WRrg53v/wAsxXOmUMx3cjrzTJ78yoEC7TnBx6UAYvxN8TrqAS1txtjHyk/3qxfD+k+TYiZ1/eS8/QVS1tft/ilYFHyLjiuwtoNsSovG0UAV47Z43AX7pq/DvB2k1JGmT0qZQqn7vzUASEYQKKryq2MZq2xCMu48mmkBm5HFAGcS1tKGJ+RuuatRlXUhuQelLcQiUYI4qpEjQP5TnI/hNAE0kAJ3Y4FVTCVuVOcKavxEkFT1NDwfLz2NAD4RgFVp4Y5wBzSKpU/LyMU4DjHf1oAim4IDdPWqbBi5OOvStCRAVANRBNwyR06UAVPLAO325qlNapJImTgA1phRliapzJtlXj5c0ASIsca4xz/KmOQeFHHrViKMkM3Ve5pwiVvmUfL2oAijiXA5y1Wo12Emo4IWEp4zmpyMHaPxoAASOnU0hQYwRSg8ClbLdDQBE2GXB4x0qsE3yHfzjoKtbARgjmkVAjc9TQA1QdwyOKhmViSRVhwQ/FI67mB7mgDOEAJLMeahktFkXkVoSRlQcCo8Z9qAMYW3mq9jMMWsnAPvWBY3lz4X1wRy52RuDg9GFdhOm5dpx7Yqlq2jjXNJeVF/022HH+0vpQB6Rrl7pXiPw9YyQuVt3UZcHmN63/B2t3WnW8Ol6tIJrST5YZs8j0rwzwZqkht5dMlbAU52muyhuJJIjb+awJ+5z900AfQU1rDqen3GnXYEsUiFSeoZT3r448eeFp/CXim60+RCItxaFscMpr6M8A+KpJ4Rpd/J/pNueGbqwp3xh8EL4r8Ltd2qA39mpkjIHLL3FAHyXRSujRuyOCGU4IPY0lABRRRQAUUUUAFFFFACUUUUAFLSUtABRRRQAUUUUAFemfBXwcPEvi1by4TNnYYkbI4ZuwrzQAswUDJJwBX118KvDqeE/AEUsyhLi4Xz5SevPQUAW/FmvfaWk0XTjulQZnZeiD0rybWH8u6WNpS4HUehrsNL1OC31rVNXCD7HED5rN/GfSuPt76C/wBffUpoc207HEX90etAFGSQrFzwT0qnqF7FaW4PJdu/pV7UvLn1KVbc/ugflrk9amn81mlX9yowPegCvpy/atba7HOO9dvFFwDnGRXJ6HbiNl4+/wA11qFlUL1HrQA8KFPBpGbKnP50PkLz1qMkuBzgDrQAROZFIc8jpVqPp71VjTJJHQd6tIOCMcnvQAh7j8qq3QIAbBNXmUDGKjZTjnHNAEEDnhutWclhuPFRLGE3DOFNSZI460ASDBX3pyncvAxjvTY8EgnpT8jadvXNADMDHXnvTeOlTbVUZJ6037q9PmPSgCCZAnIHWqE65xmtGTcUyRk+lU3XIKsPfNAE9igltpV9BUcWTGUPanaW5WR0/hIp0YG4r3zzQAqAoRtqR8Ac9aeVwcjrQ5AFADFAZRxgU0DGQOaXKqvzGmIW3ZXp2oAaDtY5P4UpIODinFdxZm4NMALLnNAAykjNCk8rjJ9acFOBSOdoOODQA1sBOfxqvLECMrwOwp+9wvQUbgyqQfwoApGFg2W5FSwM1tcrIPuNwR7VYZPlI60xlJXHU0Acjr9oNC8UwXkIxbzndx79a6877eaNsEbgHT3FZXia0N/4dZ8fNbnIak0vVZdR0K0eTmS3+Td7UAb5vpEuxeW+VukHbvXsngbxKNa00QXWFnUY2nuPSvHNRVRaQXkGOg3YrR0rVJdL1G0v4iwgJG70BoA5b40+Bv8AhGvEZ1K0jxYXpLDA4R+4ry2vs7xRo1p4/wDA8sICs7x74m/uuK+OtQsptN1CeyuEKywuUYH2oArUUUUAFFFFABRRRQAlFFFABS0lLQAUUUUAFFFFAHYfDLw0fE/jeytGUmCNvNl+gr6Y8fa3Fo+jx2ERCmQBQB2XoK4z9n/wwLDQ59cmQCS5+VCRyFrW1CwTxX44LSyj7JbHkZ7CgDm/FMiWvhKy0kII2uG8xivVvrWLCqWlg0oUAqNqA1b8WTpqPiqVom/0a1GxF7VlXUj3pjhjGGPyqo9aAKZdU824dsDH61javsltYtx5Y5A9a3dds49M1Cx0eR8zSDfKvpVC/ksobstMARF9xfegCbTbVLPT0muCEZugPWtGOTjjkHpWSpN6gubt9v8AcT2qxHeNH0UEDpQBdLg/Jn5v5URrjCjnPWqtzcK8aXMS45w6ircLHaGI2gjjNAFpEAAAp27yyWJ/CovMRAN7ioHnQuduWoAueYGAPc0oG/5WPNVGfIUrkHvVqLhAQMmgBdgZguOlKQAp4oVWY0/Z3HPrQAKAVGB1pCpzsUcetOQ4+6Ka5xwDzQBIF3EA9BTSoV95OR0pygiMnPNVndsqCKAJH9TVaRdwO2nOTu5NV7idYYyR3oAj84WwBz1P5VMu4SAg/e5zXP3c8k9ysUWTu6+1dBbLtSNW5IFAF5Qdu7qahkG4E55HarCthdpHFQSsN2B1NAEagyA7hTgVTABpyKUHPQ0hKvg4AxQAp2lvemgbXwR1pVXLBulOfAYkde1ADdw3EZphK4x1NVZS5cnqR6UhuY/lwcE9aAGz79+RwaXZ5nKcMvWpGAwCfzoUKFO3rQAIxxluM9qGGPrTiAUBNOJ34GKAM7U9/wDwjl2gHBNcz4avS8ptlG2MDn61114zS28lpt6iuV8MW6tqlzbBf3ynIHrQB1du262eJm+UdBXSeCjDrsk2gXAVXA3Kx7/SueDRNdRgLhW+V/Y1G89zpGqxalp//HxauH2/3x6UAev+ELq80DWpNA1GFooZfmtXY8NjtXl/x88G/YdUTxDax4huPlmwOjetejX9/J4u8M2XiSyIR7X5ig6ow61u6vZW/jr4fyJIgZpoSQP7rgUAfGFFWtRspdN1GezmBDwuVOaq0AFFFFABRRRQAlFFFABS0lLQAUUUUAFW9Ls21DVLa0UEmWQL+GaqV2/wq07+0PG1puXcqMCfzoA+n10S407wPFpWlSLC6QgGQ8Y4ya4zwnpV5i5lLkk5VpM9a7Hxxa3L6UsttdPEVITy1P3smtDTdLt7Dw+bQNuxES7d8kUAeE6r5dpfyxA5YuSxqfQ5ILbVI7qeMvt+aMEdTVe00OaTULm8upt8ZnIUE84zWv4tvLSHUIE0mMYgg+fjvigDzPWtelvvHd1qEo+cNt/3RWY9w97q7HduG7NUp5iZbyZuXkYkn0rR8J2RllMjjIJ4oA6WO0aZQ0gJGOB6VZNipAGSprQRQmAAMAUry5UZQYoA5yVZ7GRgWyOo9DUcdze6hMOTt9BWvqKrcw4RfnHajSY0hhOFw460AOt7XCZlJPtVuKJewxUgZWHI61NGF7YAFAEIVc5PUVPEPl4GBQ0a5B71KAoz2oAQls4BpFJXc2enWmncvX8KdgCIEnr1FAAjBiW6UxvkIJpSdpwenamEEn1oAkkmxgA8mq2/cxOfm7CpAFGGfqam8uNB5xXgUAVYx5pOAcjrVS5bzGEQQ7vcVr6ff+VO7mEFD04q65S8BdoEjz0agDldOslhlkkkGST+VaIzuyn3atyRR25YZDY9KiO14yVGDQAxS27huKSQjfz+dMYEDcPxqMEbcNnrQBYByoBPFKNqnp1qAvhcCpmO5FI5NAD0cbSAATTx84xjJx1qNQE4x1609CEPsaAKUxaKJiByTzVWOBZskj8a1JYPMJ2n8KrrGEkK4wDQBVeKZIwqtuAqsk8kcpRq0mO3PpWdqls08REWVcDOR3oAu5RbbzhMHHcDtUS3kSLvByT0rBtCLYESyHngpmrEjxqoKqQpoAuzXmXVgfnz+dYdxDNa6yt/ZkrJ/GB6VJNM9u6uPmBPX0pk9zLFdJcDkrgkeooA1ba7EjOhbDPyM+taEYbAeXOVHfvXN67vlnh1Kw+WPAZkHY16TocFl408NR3aOltLbJhx03EUAUfA3iptF1a60G7GNP1EHy2PRWNegfD/AFiPTdQu/Dt3LhzIXgz0YeleP3UYRiXXL28oKt7ZrttfRtOutC8Q27B0kVdzr/CaAON+OXhU6T4mOpQx4gueTj1ryevrX4maRD4s+HpvI8NIkXmKR/n1r5LZSrFT1BwaAEooooAKKKKAEooooAKWkpaACiiigAr2n4A6X52sSXhGdp/lXi1fQ/wCg22yyDjIYn3oA9L8W6hsv9N02FfMubhyUT07ZNR+J7//AIRfwZdR2zGa9MRAA5JY9T9Klt7Vbn4i317KAVtLRI0z/CTyT+VZmsxw6X4f1G6ZzdyTyFo3fnj0oA474f6ZBNZPe65cEeUpkMZPU1yer3hN1qN1bp+6lyFB7LV20muJ4p5HYqG6qKztbH2XSZXk+UMuBQB5zcIhtT5bZMj16Dplnb2thaiFcSBfmrhNOg+0zIiDMYbNeiWsPlRpj0xQBZCjac9TTiobHHQUbTmpIwGY/SgDJu18pi4PPU1W0/UUmuWYjC9CPWtDUowbWQgc1jaPbF2Zjjg0AdFEUbkDipFTeOOlRiMLHszyRTreJ0jKs3zdaAJljO8Nnp2okHPHSnR7gpJFOaIBQQcr1oAjcgqF79qH2qgBPJppAzkcmnMMjBWgCOX5gCozimM+wDYMsetOAYISOMnGKtWVqJZAH6LzQAlvYPLiUjPt6U+e3JTywwx3FWLq+EalY8KOnFUIg886qzcetAFqCE2qeZKgKDp71Qu77zCygbfQCtPUTvgWFDwB1rAuIihBz070AR79pLlj7g1esbiI4Vu571nsQTg80kfygk9ulAGrqtrJbSJKOY3FUDsdNwPIrpbBotY0d4H4lQfKa5qSF7a4McgwQelAArjOcdOoqzH8jAgfeqKLY0xGOtWAPLGCMnsaAJFUyBhnnHFMU4QjHTilVimQvU08dgByOtACYwB70jEOMY5FSYLZz17VE6naeeaAICuCd3eoJQCCc9BVl1+7k8VWnZI0f0xQBhpaRyXLk9c8VcudPfYrhCVx27UaLbtcyzXDD90h4rUeeZUIUDHpQBhW+nbo5DOwVAMjNUr5YmgRouQOCa17i0kkhPON3XNUVsJ4rAB4yy7+ooAwJL6WzPlEHyn4INbHgvU5LUXVskjbM7gAad46s7eLTbSa3Ta2BuxWF4Om26yUJ++uKAPQtRijbTzfIw2uuCvvWppFtf6p4I2HMkKsdntWTa28lxb3EIBYKT8tdz8KbyJvDWraRLgTwEuobuKAOg8DM2peArrTZ2zLEGQg9cY4r5d8WaW2k+JLy2K4G8sv0Ne+eD767tfFbToCLWSTy5k7D3rkfj74Z+w6zDq0CjyZx8wHY0AeL0UUUAFFFFACUUUUAFLSUtABRRRQAV9G/AJg1guDjajZ9+a+cq+g/wBnqcPHPEeqK2PzoA9Hi1dbPxB4ijWGSeUbGwgzgbcc1m+MZpLL4fQSIoMjnOPTNRSyatovxC1G48lRb6gUjj3D7w9ad8Tr6ODTrPSkXLt8xHoKAPObFZ5rcbBnPzNXH+O9UYhLcP8AN0K+1ds8h023Kq20bMt7V5ZI39u+KeMsm6gDq/COjrFp/wBumTCY+UHua6FVO36805CFtIrVAFRByBT2HzAL0oAh5JqwE2qNvWoNpBJzVmM5Ue3egCldr/oko745rCsXFswDHBc9K6KXEjFc/e4rDns2OvrGOUjXJxQBth1KrjqamXIy2MnFVY9pTJ6g1bJ2hQO9ACDcWA/h9KJQ0Q3KeD/DSvKUUlVye1RxEIA8h3E0ASFCqIxOCaGDN82aY7s78nipIsYJzkUANd1XDMPn7Cr9q6JA3OWYVlXJPnLmtK2C+Vt/i9aAM6YO27jnPSokklhkXIbNddouhrfXLOzAhRnbXXReErK9tDiMeaOjY6UAee2kNxdxM0UROOtU7jTZX3gqcda9KHh+509SsYCoep9awb3TZojJNIwVfSgDzmaFoW+Zcdqao+UqM1vXFl9pmbad2KpGzkWUx7Me9AGp4RjElzJGQdgH3qreJoRHOWXrnrXZeE9Oig06SeRQoI/OuZ8WsrTABcDPFAHPRAqYywq/vB/3e1UYP3pwx5XpVtAWxu+7QBM6YXK8+tIrYU+tNVsHAOfagsp4Bwe9ADxOSwBGO2aYRliewpcoYioHOeDTmIIAAxjrQBUmyxIHSoBDHNII5DhT1q3IVSMnuapTyLGm4kAigBrXCIWsbRdsYPJ9aczuikHkimWZSZHkHX1qyVDAfzoAqMjXA3u5HsKsQ3D2K/dEiHqpoKBTjvTWG5SpPWgDn/FN3Ff2TKnAxwPSuM0aXydUhOcfNjNehXOjR3KsicEjj3rzu6iNhqjIwxsegD2LSZ2t9ShUj5Zxg+9a3hxhpnjS6hHCuOnrXNWF150OnXH93HNdNamGLxZBcyn5ZQMH3oAuxwX41u8tNNjLSXB3FR/DWx8XNNW88CQQXHN1HHkn6Dn9a1tHiis/HAkyN9zCQKp/Fu5WPSTGf+eLE0AfJVFB5JooAKKKKAEooooAKWkpaACiiigAr3X9nuQC9mXuSw/SvCq95/Z6gxcTy467v5UAepapO138R9M09l/dQwGf8ea47xTMdY8ezRA5S1XAH0rsUbd8VbhmHyw6euPbJrzkXRm8R6hdA7WkkYL+dAHIeNNTNtps/aWVti/SsPwXpojZ7pxliO9N8b3LXviOOxUgrGctj1rodGgFvbRqBwaANiEDczN0qQ7T0PFRqAxYqfwpSNoHHFAAO+OlTnaQFA7c1ACQxXtUud6ADpQBVuAVQr37GmaPHG9vd3DtmQfKM1LOpAYk9qoWOU3DJwx6UAX7dMKOOasAhm6dBUcJDk47VIhIBJoAbnc4AFRSntjgdqmkGxQ3rVWRjuH60ASx43ZIzVhI9w4GFzVNXBz61Yjc42jrQA26Ri27b0p9vO0YEpGQKlBaf5T1xTNoQbT9MUAdPol60bLcQ/x8EV3VlqQt7ZewJyRXmWgyPFeJCi7gTn6V6PFaNKgYADA6UAXX1MSyHzBmMDiue1XSTqEgmimOGOCnYVry2pWAyZ/Cn2NusdvvDbsmgDmrLw39lkkZ8HI4Nc7qNlcrdnYoPPavSpZY0hk45A4zXCXLvFevcOf3foaAJzdNZaSgmYBvQVxGuXhvbzcDx2qzqeqPdTMin5D0rJZN7DPLUAPt0VX3DririuwQjbwaSCNRlWAp0koC7SOB6UAMQoCdxwT0poG1+e9IqEvyOO1N+bzgetAFyADeZAOB2NE/yuGA60Rj7wzTHJZTntQBVnZsYx1rK1JS1sGXqGwa2H4TkVSmjDxNGe4oAo2lxFFOLbuwyK2D90Y7Vh6bp8TiWSZytxE3yA/xCtiAEgl+hoASRjgnvTFB796kdRz6Ug5GTQAirsljIOcGuD8dWYt9a81RgSDNd8QVXIFc946smuLCO5AyyDNADvCMxvtDZQfnt+grqH33Fna3cefMgYE15x4HvTb6k8JPySrjFepeHhHLcPZSH5X+79aAOos777R4x8NyKTvkBDj04qH42TiHTpznn7PiqUNpqieLtJj02LzJraYGUH+FO5qb4+qBo6up5MeGH40AfNNFFFABRRRQAlFFFABS0lLQAUUUUAFfRf7P1sfsDTDoobP4186V9K/AR1XSjGDyY8kfjQB1l3MbXxzrlw3AGnqF/KvKxMSzSDhvmbNeoayok8VayOpFmvH4V5PeTCLTbqRflKIaAPPxINS8VO2TuZyDXfWwCDyx/DxXnXhcmTXxIeeSTXogtbnzyyxna3IoAkZsA4ODntUgmIcZPy4qGPfuYOhFPaEuB2AoAtQupRs9TUqnbgY4NVfJW2gEzTAgn7tTxyKygqcigAmUlWGM8VRhj2gZJ5Nai4I4700wILEn/loGoAihRRu5x61OMAcVEBzgdxUucIAO1ADZhuXJGPSqM3UYODWisgZMY49Kp3SqzZXt1oAiTgcHJqaMhssD0qsyFTxmnRll5zx3oAtxyMkgZeRUzOHDFR9aqIC/3TxU0YKxYHU9aAOq8FRRSyu8g57V6HBCykuxIQDgV5n4NuFg1oRyHCP0+tepJKPM8tsFRQBTlIUhZCQppyqkcJCnr0FWJYRc8qMkfdFU9QuYdJ2NdcB+PpQBneJL2HTdM85x8x6V5nqviBryHYoxmt7x3rUV5HHbQMG7kiuC2ncD1AoAsxK68v3qWNdrb2702MFyC2elWVgLRnPSgBhcCUbTwe9Kw4LdKRo1hhBzlifyqIyjoTkGgCdXJU49KjBL01Mk/L0pyxkE5OATQBPESJOlPYAnINIg2cE5460F1BHv+lAEEvGf5VUkUAEAc9auTAKcjmqpO/zMDtQAw2pYpclcbuKtbcY459KTzt1lFFjBU07JJBoAieMk5JpqYPXpmnNkPz37UjfKCvWgB5GTtB4qvqsX2nR2XrtBBoDso56U/wA4eW8BH+sFAHlmlzGx1yM/3ZMV61ZyiPU7GdThd4JryHUo2ttZmUcFHyK9Rsp/tWkWk4IyUHI9aAPU0uG074g6bPH9y/Xy29xisD4/ALpCkfxIM/nWjY3cV5L4XmZv3qThcmqfx8w+ixoV/wCWZIP40AfM1FFFABRRRQAlFFFABS0lLQAUUUUAFfSvwEgA0ppN3zBP5181V9D/AAIvj5ccCkbWQhvwoA6fxHM1l471AEnZc2Sn9MV5H4jWQeG7t0fGWIJ9a9f8drnxZCFT5vsTZPqMmvF/F5dfCwIOA0pGPxoA5/wDbC68Qqh6BcmvULm4k+1GNcKiDArzj4bMF8SNn/nka7zz1d5N7fNuNABIzsCxIyKEO7lu9NXMkoiH8XelKvBI0J+bB60AJLEhBGOO1EabIwy/iKmO3GCM4phbaNw/KgCVJBtwPzp5JwD371URxvLKPk7j0q0o3AMTQA09SR37U5GDjGcU1uJM04qOo70ANKlGyDxTVQOST1NPDYbkZFOVCr9OGoAimiAT5fSqQB3ACtNsbmXqAKpiNWY4ODQBMhCKABz3qYEhCSMZqMLtCljketIZCwJ7UASW8jxTK6NhlOQa9C0PxNb3kCx3D7Jl4JPevOgwO3HUVJk5JRsN6igD2y3vLa1t/PeZcHoM15/458XWN3Gbe3YO4PPtXF3uo6jt2faWKAdM1iKu6bc2SSefegC9uab5yST71bjhwm5Vye9NiUMq5XAFWfNCDaBkGgBqghBx1qQE7CGHSoy5APYUwtuULuwPWgBJVDqB0qqQAfcVecArjOSKgePkNjk9qAGQeZngcVLk7iG4pVYqw4+WnTIGUMOlAChucE8etAZCxz0qNFITB59KkVAfvdKABwGj5OMdPeoBw2duM1KwDck9OlCDevPUUANCAuB2pGHBVT0p7AhcfrUZIyVx170ANJ3DJqM59aex+Xyx26e9QvIFX5uDQAjEhenSmwZuLxFA4HJNCrLOdiAnPerSpHZQMiHMrDk+lAHCeJrKNvF5VV+SRM/U4rc8KytPo7Rc4hcis7xQDDeWN0OoOCam8GynydROcfMTigD0zS9zXOhOBgJdLj861/jpEz+H0Yf3GFY+iZkGgQE4LXSsT+NdB8bVz4dU9wrcUAfKlFFFABRRRQAlFFFABS0lLQAUUUUAFe2/Ah0+3RozlTvJFeJV6V8KNTFnf/ewY5A34UAe7+OykGu6PM44kWSImvC/G4MWgSwkcJcnH0zXunxN2v4bstTTkQTq+R6MK8Q8etv0ibHKs4kBoA574dRs/iCQqOkRrtUt/wB7Kzf3jXOfCxVFxqEp+8I8A10aedG7MTuBbJoAsKmBkfnTQW5BOcU4yg4YDA9KVWUnpQAvRd2eahkk2jjlqkOefQUir8pfrmgAthgsWwAwqzGxEeMZ561CFDYNTB9q7QOaAB1DH6UxCGkCM2B60yZmVC2cGmQukjxj8SaALEp8sYAzmo1mbds6+9S3UiFwFPygcVEM44AoAkyEGT1NKsakk8A1AZVaPHpTCJLQB3BaJuQw7UATj5chhkU1njC9KQXccmckAdjSFRIvyn6GgBwf5cEc0YKgEE4PWgYPDDp1qUqRjH3cUAQThWiJHAFYgYC768dq6BlXAU8561j3KRR3wUjvxQBp25Gzk8d6lAXYfQ9KSOPCBlHbpTj0BAoAgPA2ueaVQM4IqbYGyT1HSoVCIxJbg0APBIO0L8v86f5LOxbHPakSQO20DgVMZG3DBwBxQBCEOcnjHWlbp7VKV5JJ69aaYwpLDkYoAiX5jt70FtmQ3NWdipaLIeGY/nWXeXEcUxJb6CgCyecNjinkLkGqcN6JAB2q0xyRjvQA58BvY1WY7WJHH1qwwGOtRSEE4IzxxQBWCzPlgMJ2NKlumSZn3H0qYBmjC54HakKjaOmaAEE3ljbENue9REBycnn1pzKFPB6UJjHI5NAHOeMFU6TbSkcrJg1D4PZBDqC4ySMip/GhI0KBfWWqvg6NxcXcbHHyigD07RADqPhm2Od7Thjj2rqvjDF5uiKPVGrF8DW32zxjZ5GUs4C+fQ1e+Lt/siMBICrFmgD5ZYbWI9Dikp0hDSuw6Ek02gAooooASiiigApaSloAKKKKACt3wjfNZa7Fg/LJ8pFYVTWs5truKYdUYGgD7Iksjrvw0e2fDu9sShHqOR/KvnrxKzz+GQX+Vo/kYH2r274U6z9u0o27SF02h0B7eteZfEbSP7M1nV7Db8s+ZoB2weaAOM+Hdw0V5coD99eld1tVjgda8s8J3LWmtpk4BO05r1HAWYjsec0AGzIwRzSNGWPyHBHWjzl3ZzUgCsMd6AIZN8Y2D5geppBJ5fBHSpjGxxzQke5jkDAoAYHJAIHB61YBG3cBUeDt4X8KeF2rntQBFMQ1s/GaoE+SsZU8mtNVBjdeu4cVR060OoxTQqcTRHKg96ALZ2tGo6EdTQh9+KqQu0haJjhozhhVlVC8Zyp70AQSgrO3oe9WbW62AxSYeM+tDqrrtP3expv2dccHFADL7TU8vzrN8nqyVQjuWETwngjkZrQw0JO0nNRS2sd5Ef4JRzkUAVLLVEL+VPw2cZrYDE4bquOK5C+ie0uAzKSp7ir9lrBRVSQ5XoKAOgGCpPftWLLHI93nGTnr6Vca+jHKsKovqUfmMOg9aANiMEAKDwBVhcDk9MVmQX8ToPnGRVDUNWkc+TCfxFAF661JUmCRngHmr+n2EupMbiX9zbL3PGaydOt4hNF5v7yaQ8JW/qc0odLYfKEH3FoAJ2tYW8u3HTq1VyQwPOKZ5ZMRcVHsZuScUASiTHA59BTpZEhtmZ2x60xDHGfl5YjqaydVnZwVz8o60ANGttfagkY+WKPpWVdSmW9lySeeKq286x3UjDr0FX7WEtIZWXOR1oAdZ3WZAucMK6ReUVunFcUrmLU8Dua7NW3W0R6NQA5jhSe1QPIVK4HJqycNGV71CFBdSRkDrQBGswVyjcGmMWJIU1NcQCO4HIZSMg1GcLyBQAgOUz3qRQvlknimhd4yv3vSlILKyn0oA5bxpKW02yQqQPN/Oo/B7ebql3jJBIUVd8cvGLHTlIG6I5PvUPw9i+0XdxMowoyxoA94+G1oq3GoXG3kBYw1ecfGXVRJNeqW5DbFxXrXgrFl4Ka7fjeXl3fyr5v+JmoG41ERht25y5NAHA0UUUAFFFFACUUUUAFLSUtABRRRQAUUUUAe2/B/XJreSyVHwu/y3B9K734zaA91p9nrNuuXtm2S4/uGvBvAWoNBevAHK8h1r6ythB4k8JiKQh0uINjk9jj/ABoA+N9Shey17KLgAhx716rAn2zRLa+Q5DLhq5bxnpf2WURyLsu7CYxTD+8ueDW94Fu0nsrzRXOSF8yEnuKAJBGPMFTrhSMVFITEzI/UUizqyKR1oAsj5jyak4Rgo71U8wjvg0nnZfg5NAF7PzjHTvTZfun0pqHeOuDTmG87e2KACLCjd19qp2Y+zeLIpFYrG45Hap4yykgdqiuoTKyzKcMnpQBW1cPZ+IpZ1/1cnUVYiZZFO3kdabdOL2MeZwyjrVaJWQfKeB1oA0EYhQMc1IQuffvUMcpdRx06mpiwz17UAMO3JPp0qI/uzuXqetSFxzxiowSrbTz70AZGpPkSRlcg8/SsVHjijO4/N2rY1eOSOTKjO+sqPTJLliWO0LyRQAsMjPGwJ+lVomZvMDg9cAetS70tAyIMk9Sa39MtbGXQTqjuoltXzsP8VAHPr5nnCJcq5HANCyPHI2R+8XqDReX7X2qNdooTP3QO1PiSSffIw570Ab/heVUvzdzDcVXC57GtdnaSaWU8ljnNZ2hRIbNomHLcg1oAGMY9OPrQA5eYjg/Wo8/LT1I5Hr1pGZY1O4ZA6UAV55kijLNwK5HUdT3yuqVo67LKy/KflNcucgkuM0APjco+8n8K7XQbiC602UlMso6VxcJD3EYbgZ5+lb11rMFjbSW9kgDMuCwoAzlbzdYwBnDYFduw2woCegrkPC1sbrVDI/O3k/WuydcybaAHoQUJHHFQlXU7h0Papc/Nj0pHbOATQBGybsUjRYHH408thR6imM5wcdDQAixgHIPSlbCq3vUZLL0NRSz+SPMcEgDJFAHH+N5/3scec4HSt3wLbPa+HGkA/fXsgijHfk1wepXT6rrrE5KtJtVfbNe5fDzQhf8AiaxiK/6Lp0YlYdt3b9aAPRvEDx+HvAkdkDh/KWIfXvXyb4quTc65L82QnFfQHxd8QLE7wh/kt4+cH+KvmeaVp5nlc5ZySaAGUUUUAFFFFACUUUUAFLSUtABRRRQAUUUUAaGiXf2LV7ebOBuwfoa+rvhlqvn2ElkWBC/OlfIIODkda91+FXiMQ3NrMzHBHluM/hQBs/HPwm5i/wCEgtRtRl2XIHc9jXl3hHUvLSG+U/vbRgsg9Ur611DT7TWdNks7uMS2064KnuK+SdY04eDfiHe6UykWkrFAD/dPQ0Ad7rdoLgJeWp3RTLuGKxow6grt4qbwnqLyQXWiXJ/e253Qk91qaaMrKyjoeaAIipKg/nSqgU9KcOFCjrUhIOPUUASIOnpUo+UHnINRxt8p4/CgSDoOaAHSKSBTAQEIzwetPJJUnselM2YIP5igCF4fToaasYSMjNT7t44pBgE5FAEaDYpxTkOVyeDTm44HJpkhKjOKAFOHGelH3WBIyKbGys2Ke5O3C8j1oAW4SGWHzOCy/pWNbHZes0v+rfipbq48l2j55qGLM8BQr82floAbdaOHuGUd+RUa6ZJBYSQFjukPAFaEM0kyjj99DwR6ipDfQdSp3+lAGOdGW0gBzljU/lm3ZEC5yORWlGRMpkk4jHQGqIl866ZyflBwKANG0BSEMBtGeRV2QN5IkHzCq6AhVGO1S7mUj09KAI4595wBhu4p0o3IaSSMuwmTgr1prZYZz9aAMW8j3gqeaxprUEcDkV0lxEH+YCs6W3JLUAc48DRtuA5pnltKeFO4mtk2+7jHIrT0vTE8zzXTgdjQBb0GxGn2O8jDv1rRU7jvprnzPlAwB09qmQAJj0oATPc9aac5yKGOAc1CrMRg8UAPXnINKWGMYyKi3fLg8f1pN2BxQBL5at161Q1SdLewuJW6BSBVrzCfY1yPi/VVMbWkZ4UfMR60AY3ha0F3rTXTj93Dlz9e1fTvgKw/4R/wbPq92uJ7kGY57L/CK8U+GXhw6rqFlYAEiZ/OuGH8KDtXuPj/AFeLT9IXTIQBvUDA/hA6CgD5/wDiTrLXVwY95LTOXbntXnda/iW8F5rUxU5VDtFZFABRRRQAUUUUAJRRRQAUtJS0AFFFFABRRRQAV2PgLUTBfNbk9SGWuOrR0K5+y6zbybtqltpP1oA+0fCl+t9ocOHLNGNrZrxL4+WtjqF/FfadmS8tBsu2QcL6ZPrXdfDnXdjDTyoIkIwc12Hinw9BrfhjUtPSGNJLmM/MFAJb1+tAHy5oesbr3TtQI+dT5UpHcV3uqQFbsMn3WG4H2ry+ytJdE8QXOj3ylCSVXd6joa9Ns7r7X4fUyNme3+RvpQBS28kk8ihCVznqelOV0YKWHNOkK+bkDgUACt8vPSlTbnIqJjucAVYUBVFAAXwMD8qCePeo2O2XIIqQNxg9aACPGTmkYDNOUA89PakYkqeMUANXkkdKCCQc/lTVO7d7U9ixUYFAEZCFgy8HoRUjfImMdajbH3x0HWnebxv60AUntfPuQ7DCrUE6Mk4dDtVa0wSQS3BNQzRq8RXrxQBnx3y/ajP07H3qz9osmPmnaDWZd2/kxqAcYOTUS+WykkdaALV1qYkVoohwfSp9LsyybnNVLeOMyqqit6MLHGAODQBKMrhaYX2Mc856U4Hk46037zYx+NAD42KxPjkNURbgcU4nsDxUbE7tp7UANMe4Ej8qhe23kdqlJc52jjvTgRjrzQBWFkgYE1cU4GxeAKRO+acuCd3TFAChdq7qfnOM0ZyKaWxwaABxwTnrUDjgLnmpW6ZHIqORSRkUAJnn5ucVDK4XBHc1IAcYJ60+2tDcXUUanO5gKAJ5rGWPQ7jUz8sca8E+teSag/2iWNQS00r5I+vSvUfiLrYhSHw5aPiKFPMuCO5rH+EXg5vFfi1b+5iJsLVt7ZHBI6CgD3D4XeEYPDHhuK8mH+mXcYZyRyq9lFcD8UNbVrrUHXKiJSAfevbdY1KDRtKluZMAIuEUdz2FfKXxF1Zp3EG755nMj/SgDgGJZizHJJyaSiigAooooAKKKKAEooooAKWkpaACiiigAooooAKASDkdaKKAPZ/AettJFZXkbYmhIVh7ivozTr1NQsYrhP4hyPQ18Y+DdWbT9U8ktiOX9DX054E15ZIVs5WHzfdPvQB5J8dPD91Y+II9ZRMKx+8o7dqy9A1E3emSzQ8syYZfevo7xd4bt/FGg3FhMgLsp8tj2NfJtjJd+C/FE+nXsbIFk2MrfXg0AdbYXTH93KhDD1q+eeneoZIhfsZ7YquRnA70+xb7TGyt8jrxg0ATRxgDJNLIy4Iz0pjt5RMbfe9KcohlgBbIkHUUARbd670yWHWpo23IG7ipEkht7d44Vy0nUmmIBgDHSgB4AI5600k4wetOzz/ShutAEeABkd+tOyR0PFMkyKFBJ5oAdt3oR0qMrtAHp3qbJU4Az60uVIPGfagCMrleTQMAYx9KG+Qj0pwK8ZFAGTqUPnKVUHNY1xa3YaIoMQjhq6oryePpUDW6zapApbEYGXX1oApRadJE0ch6YrWj4ABGT2p0z7pWI4XoopXXbt/vYoAQqB9TTd3yEZoyCSCaa/3eBwaAEUEgkUfeHPWlBK8YzRjvmgBoYpkD8ac7DjjmmnAOMc0pXLA+tABGvHzHrT2I4ApMY5HX0pJOMHoaAHH7uQaaTvPNM3MBilG057UALnb8p/KkLHp2FOGGGSORUbE56cd6AHEArkHmrlhPHpltcX8vJjU7M+tUo4y8gRO/f0rI8XapHBZG2jb5EHJ/vGgDjdUvbjUtRkVcyXV3Jg464J4FfVvw98ODwl4bsNNW3O+SPzZpR2bHQ14n8DvCP9t+IzrN7EXgtvmTcOC1fQ3iPUxpWjzShtshGEoA4L4ieIxM7WCriKFssfWvmnxBqH9p6xPOPuA7U+gr0Lx1rzi0ldn/ANIuDtH07mvKqACiiigAooooAKKKKAEooooAKWkpaACiiigAooooAKKKKAHRyNFIsiHDKcg17R4I8SEwQzq+GOAf9k14rWtoWtSaRdZ5ML8Ov9RQB9qaFqf9pWCu5HmgfNjv715v8Y/hr/wkdk+tacn+n26ZdAP9YBWJ4S8ZyQCCW3nEkfYZ+8PQ17ZYX9vqdmk0LKwZfmXPI9jQB8ceG/EsukXq218rGNTtIbqtdxqBt9PlQi7R4rtd8bqfun0Nd38Q/gnaa/52paIy22oHLGM/cc/0r5+ubS+0i+k0zWFlhlibbsc9PpQB6TBGL+yN0rbpIjhsVKFGB6muQ8P6tcaBqKxS5ewuRgk9q6+WB4n+R90T/Mje1AC7dh4o3AHryajUuOCacoQupIOBQA8MQASOalzjmmZ8xyxGB2FObBwAaAI5DgbqQMMjtmn46hqYVI59KAJOnIpD0z2pEYup7U4AYxQBEwzjnpSEknGOKcRzinEZT3oAZkdjUMLg3z4I4FOKNux2qokLW+p8tlXFAGnuG719KaxOcseT3pu4AYxSBjjb1NACOcGgcL1yfSmkYbJpQCx44oAcWJOQKbjdwDTs4OPTvTSRQAu3IB68805vboKaCVbgfKace3pQAgIUZPNNc7lyTzSvjjHaoycjAoAcvIx39aUhQvvUY3ZwBz2p7I69VOaAF3gY20NHL5bTbSEHGfU0j3EGn2r3l11HEcY6sau2v2qOGG51IKsbjzFgHb0zQBXuIjaWYV22SyruJ/uivPporjxX4gh0uwUuobbkd/etHxVr9zq+o/YLFWkuJmCBU6/QV7b8KPhevhSzTUtTUNqco3bf+ef/ANegDr/BHheHwj4ahsFx5gG6VveuN8e6yLm7ZEmDW6DjHau08Ua9Hptq0EbAzMvP+yK+dfHfiDyrZ4Ym/fT5HB6CgDhvEupnUtXlYNmKM7UrHoooAKKKKACiiigAooooASiiigApaSloAKKKKACiiigAooooAKKKKANTRtcudHuA0ZLRk8of6V7R4P8AGUkiR3FpcYbPzKT+hFeB1f0rVrnSLoTW7/7ynoaAPtfRfENrrEYVWCXAHzRnv9KwfFXwy0TxXBcm7Vlu5eUnHVD/AIV5P4Q8bpdBZI5PLuE6c8qa9p8N+J01VRb3BC3IHDDo/wD9egD5n17Qrzw/fy6DqBxJF/qZD0cdjWx4dvHmsxY3TfOn3Ca9w+IngC18baRtB8nUYATbzD19D7V8ySHVPCety6frcMkU0Z4LDr7j1FAHoDLhsPwRxSgBSKraZqdrrdoGjceevBGetWclGwwwRQAjtycUitxkUMCckdaiX5SSOlAE3JPvSsOMU0ZYZ70FsA5NACBwppQxPPeoyTjpQoPBB570APlUvH8nDDpVf7Q0OBLVgkjvioWVXU7xmgBTIHIZTwaoapOYHimPY4q1bxeWxyflPSmSaQ+vala6TGxDytnI7UAWgolhWVD1GTTkKj7oyfWg2U+l6lNpdyCr2/AJ/iFNbg5HAoAVs96cCMDFV95dsD86mBAFAAec0gAwM9KFyCSe9KpUDnrQApJXkUm4tzQxAX1qLePujigCWmgdaXcW68UHaw+U0ACrhgQelSPdOis5wyqOapz3SWibpSMdh61yms+JrpMxxRFYu5oA3o57ea5bUr5w3l8RQ9h71g614ru7+5+zWJeWRvlGOcewqtouja54yvVttNhfymbDSY4FfQ/gf4Q6P4Yt45rtBdXvBZmHANAGB8IvhYdJePxFrSh71xuhjYfcz3+tei+JfFMGioYI2DXTDgdl+tVPE/i+DTImtLF1a4AwWHRPpXiXibxfFYmR5pPPun6DOTQBoeKvFiQpLNcTEs/JGeWNeMajfy6levcSnkngegpdS1K41S6ae4cknovYCqdABRRRQAUUUUAFFFFABRRRQAlFFFABS0lLQAUUUUAFFFFABRRRQAUUUUAFFFFAEtvcS2syywuUdT1FeteDPHAuBHFLJ5dwmO/6ivIKkhmkt5lliYq6nIIoA+ztA8WxXaR2986pM3Cv2b6+9J448AaT4503yrxBHdIP3Nyo+ZT/AFFfPfhLxuZmFtfSBZeisehr2bw14wuLfyre5fz7cn5nJ+ZBQB8/+IvCfiP4dati4jfyM5juEGUcf0rodA8WWesIsN86w3I4DE8Gvpq4t9M8Q6Y0U0cN5aSjBDDI/wDrGvFfF/7PyMZLvw1cFG5YW0h/QGgDPeEqMoQ6H+JTVJyBJs6NXBT3fifwpM9ndpLGEOCJASPzrc0XxRb6k4juCIp8dT3oA6RSOFzyKCwDHI4pB0DdfcUMRg4oAVSCjYPTpRC3BJqGDknHTvUpwile9ADJnbpSKWYU3Dq2TyKQSgEntQBLg7GIHzDkCup8OWcOnQPqU8ii5mXCN/zzrz3UNYMGBGpLMcDFdZpEN1LYKs6MysucUAWPFuoR/wBnxTTjzbrOFnX+Ie9c9HKs0CENzjmr2uK50h4V5MZyEPUVjW+nsbVGWYqzjJHpQBfBCLkYIHWlidSckZFVEtJY2AaXcvpVrO0DA4oAkc7mGDioSQG+Y89qUHJo2butACk570xkyBg8inbtvDDgUPtRS8rhIz3JoAhW4VmIJ5Han3DSJCXjAUEcE1nT+JNH00nH72Wucl1XWvE98tlpVtLKXbCJGuaAH3eqNDdmO43SPn5AOcmvQfBfwxv/ABZcRahrMLW+m4+4wwX+ldR8PPgymnPFq/ijbc34wUtycrH9fU16BrXi7TtFjMMTLLOvAjTotACaB4a0DwDpckVmfIgZt7NK2ST7Vynij4hLLFJBYsYoRw0ucE1yXinxn5u+a/uf91M8D6CvIdc8U3GpFoYSY4P1NAHQ+JPG+N0Fm++XvJ6VwE88tzK0szl3bqTUdFABRRRQAUUUUAFFFFABRRRQAUUUUAJRRRQAUtJS0AFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAAJBBBwR3rr/D3ja503EN27SRdA/cfWuQooA+hvDPjSRWjk028BQ/eiJyD+FeraV4u0+/KRTOILg8bW6E+xr4tsdRudOnEttKUbuOxr0fQPH0E6xw3nyS9Mn/GgD6X1Tw3o2tqft9jDOG6kjrXkHjX4CQTSG88MOYG6tAx4H0rQ0rxrc27K9rdeYo+9GxyDXcaX48sbxxFdobaTH3uq0AfNtzD4q8GTeTq1jM9uvG/aSPzqVPGFjdIEGYnJx81fVc1vp2uWRSVIbu3cd8MK8z8U/ArRdWczaWfsch6qOlAHB2u1oFdGDDGcg0jsQTkcetUdV+F/jfwsWNgzXVuORsOa52XXPEdl+7vdNk+U85Q0AdduLL0J+lHlSS8BD+VcnH46vYQQdN59xT4/iDeLGVNkdxPYUAdrpPhJLu4W7nPyIchTXfw2lssI3MEYDCivD28ea9OoW1s5FA9FNRt4m8XyMHNtPjt8poA9Y1vRBcWsk8QzOOw7iuQCMBs2ncvUelc63jHxdFIr/ZZcAdNpqlP4p1+SQyiwkVm6/IaAOxORjIxTSCeAOK41PFGt7sPp7sT/ALB/wqePVfFF24+z6TKewxGaAOtVDggcfWomurWHImuEQD1NZUHhD4ga6xxayQKfUYrXtfgL4kuNUtVu51NoxBnkZ+VHoB3oAwNQ8Y2duWhsYzcSngEDjNZ1n4e8Z+LpCLexuWjznkFVFfSGh/Czwj4ZCzi0jkkX/lpcHODWrqfi/RNCiCRskjdo4AMCgDyPwl+z27sl14lu8Dr9niPP4mvV7HS/C/gKwYWcEUJ9sGRq43VfiNqNyjrAUtYj0I64+teca342toS+64aeY9ec5NAHp/iH4g3E1u8UGLWP+8DyRXj2v+OI4mZLY+bOerZ4FchqviW/1RiGkZIv7oNY1AFq91C51CYyXEhY9h2FVaKKACiiigAooooAKKKKACiiigAooooAKKKKAEooooAKWkpaACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAvWOsX2nuGgnYexORXYaX8Q3UrHepx3YVwNFAHvmjeM/K2z6bebCRym7g12un/EqeJAL2BZgf4k4NfKUU8sDbopGQ+xrZtfFuqWyhfNDgf3qAPrK1+I2iXHEhliPoy5q6t94Y1bG77HIzdpEANfK8HjtjsE9vyO4rTi8c2TsNxdD2PpQB9JzeGPCskg8yzswx6DIGaafBvhNn3f2faZHYEV4EnjGzlAY3zbgOCW6VKnimFz8t+2T330AfQUOi+G7cbI7SxX24qcJobMIgtkSOi4WvndvFNsnAvCW/36F8UW3mg/avm/3qAPodtP0JAXa3swO5OKpu/hRVJYWGB1+UV4TceKYUQt9rO09i1ZA8XacjlWuN340AfRguPCiqtwBYjsDsGfyp48QeG7YNsntk28/KgH9K+bpPGmnc4kOO4qm3jaywThzjoKAPou5+JGiwRsyCWQg4wABWJqPxPkdMWMCICPvvya+f5fHL/MI4Cc9Mmsq48V6lOm0OqD2oA9a1rxq8u5rvUGKdSpauH1LxxAhItFaV/wC8elcLLPLO5eWRnY9yajoA1b/xDqGoEiSYqh/hXisokk5JzRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFACUUUUALxRkUlFAC5FGRSUUALmjNJRQAuaM0lFAC5ozSUUAGaWkoFAC0UUUAFFFFABRRRQAUUUUAFFFFABRRRQAUAkdDRRQAUZNFFAClmPVifqaSiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigBKKKKACiiigAooooAKKKKACiiigAooooAKBRQKAFooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigBKKKKAP/2Q==");
            attachment.DataElement = new Hl7.Fhir.Model.Base64Binary(bytes);
            attachment.Title = "Computed tomography (CT) of head and neck";
            attachment.Creation = "2020-07-09T11:46:09+05:30";
            attachment.ContentTypeElement = new Code("image/jpeg"); //= new Code("2020-07-09T11:46:09+05:30");
            attachment.Language = "en-IN";
            media.Content = attachment;
            media.Modality = new CodeableConcept("http://snomed.info/sct", "429858000", "CT of head and neck", "CT of head and neck");
            media.BodySite = new CodeableConcept("http://snomed.info/sct", "774007", "Structure of head and/or neck (body structure)", "Structure of head and/or neck (body structure)");
            media.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            media.Created = new FhirDateTime("2020-07-10");


            return media;
        }

        // Populate Composition for DiagnosticReport Media
        public static Composition populateDiagnosticReportRecordMediaCompositionResource()
        {
            Composition composition = new Composition()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DiagnosticReportRecord",
                    },
                },
            };

            // Set logical id of this artifact
            composition.Id = "254211bb-0b56-4f7b-a55e-100253c68c71";

            // Set language of the resource content
            composition.Language = "en-IN";

          
            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://ndhm.in/phr";
            identifier.Value = "645bb0c3-ff7e-4123-bef5-3852a4784813";
            composition.Identifier = identifier;

            // Status can be preliminary | final | amended | entered-in-error
            composition.Status = CompositionStatus.Final;

            // Kind of composition ("Diagnostic studies report")
            composition.Type = new CodeableConcept("http://snomed.info/sct", "721981007", "Diagnostic studies report", "null");

            // Set subject - Who and/or what the composition/DiagnosticReport record is about
            composition.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");

            // Set Timestamp
            composition.DateElement = new FhirDateTime("2017-05-27T11:46:09+05:30");

            // Set author - Who and/or what authored the composition/DiagnosticReport record
            composition.Author.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147"));

            // Set a Human Readable name/title
            composition.Title = "Diagnostic Report-Imaging Media";

            // Composition is broken into sections / DiagnosticReport Media record contains single section to define the relevant medication requests
            // Entry is a reference to data that supports this section
            ResourceReference reference1 = new ResourceReference();
            reference1.Reference = "urn:uuid:3c598ce5-d1db-4d4d-b5e2-69f142396d55";
            reference1.Type = "DiagnosticReport";

            ResourceReference reference2 = new ResourceReference();
            reference2.Reference = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";
            reference2.Type = "DocumentReference";

            Composition.SectionComponent section = new Composition.SectionComponent();
            section.Title = "Computed tomography imaging report";
            section.Code = new CodeableConcept("http://snomed.info/sct", "4261000179100", "Computed tomography imaging report", "null");
            section.Entry.Add(reference1);
            section.Entry.Add(reference2);
            composition.Section.Add(section);

            return composition;
        }

        // Populate Composition for DiagnosticReport Lab
        public static Composition populateDiagnosticReportRecordLabCompositionResource()
        {
            Composition composition = new Composition()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DiagnosticReportRecord",
                    },
                },
            };

            // Set logical id of this artifact
            composition.Id = "8a29f8cc-c494-4e2d-ad2a-7ca80ced4741";

            // Set language of the resource content
            composition.Language = "en-IN";

            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://ndhm.in/phr";
            identifier.Value = "645bb0c3-ff7e-4123-bef5-3852a4784813";
            composition.Identifier = identifier;

            // Status can be preliminary | final | amended | entered-in-error
            composition.Status = CompositionStatus.Final;

            // Kind of composition ("Diagnostic studies report")
            CodeableConcept type = new CodeableConcept();
            type.Coding.Add(new Coding("http://snomed.info/sct", "721981007", "Diagnostic studies report"));
            type.Text = "Diagnostic Report-Lab";
            composition.Type = type;

            // Set subject - Who and/or what the composition/DiagnosticReport record is about
            composition.Subject = (new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134"));

            // Set Timestamp
            composition.DateElement = new FhirDateTime("2017-05-27T11:46:09+05:30");

            // Set author - Who and/or what authored the composition/DiagnosticReport record
            composition.Author.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147"));

            // Set a Human Readable name/title
            composition.Title = "Diagnostic Report-Lab";

            // Composition is broken into sections / DiagnosticReport Lab record contains single section to define the relevant medication requests
            // Entry is a reference to data that supports this section
            ResourceReference reference1 = new ResourceReference();
            reference1.Reference = "urn:uuid:2efefe2d-1998-403e-a8dd-36b93e31d2c8";
            reference1.Type = "DiagnosticReport";

            ResourceReference reference2 = new ResourceReference();
            reference2.Reference = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";
            reference2.Type = "DocumentReference";

            Composition.SectionComponent section = new Composition.SectionComponent();
            section.Title = "Hematology report";
            section.Code = (new CodeableConcept("http://snomed.info/sct", "4321000179101", "Hematology report", "null"));
            section.Entry.Add(reference1);
            section.Entry.Add(reference2);
            composition.Section.Add(section);

            return composition;
        }

        // Populate Diagnostic Report Imaging Media Resource
        public static DiagnosticReport populateDiagnosticReportImagingMediaResource()
        {
            DiagnosticReport diagnosticReportImaging = new DiagnosticReport()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DiagnosticReportImaging",
                    },
                },
            };
            diagnosticReportImaging.Id = "3c598ce5-d1db-4d4d-b5e2-69f142396d55";

            diagnosticReportImaging.Status = DiagnosticReport.DiagnosticReportStatus.Final;
            diagnosticReportImaging.Code = new CodeableConcept("http://loinc.org", "82692-5", "CT Head and Neck WO contrast", "null");
            diagnosticReportImaging.ResultsInterpreter.Add(new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24", "Dr. DEF"));
            var link = new DiagnosticReport.MediaComponent();
            link.Link = new ResourceReference("urn:uuid:35e0e4fa-1d49-4aa4-bd82-5ae9338e8703");
            diagnosticReportImaging.Media.Add(link);
            diagnosticReportImaging.Conclusion = "CT brains: large tumor sphenoid/clivus.";
            return diagnosticReportImaging;
        }
        // Populate Specimen Resource
        public static Specimen populateSpecimenResource()
        {
            Specimen specimen = new Specimen()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Specimen",
                    },
                },
            };
            specimen.Id = "6fbe092b-d72f-4d71-9ca0-90a3b247fa4c";

            specimen.Type = new CodeableConcept("http://snomed.info/sct", "119364003", "Serum specimen", "Serum specimen");
            specimen.Subject = (new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134"));
            specimen.ReceivedTimeElement = new FhirDateTime("2015-07-08T06:40:17Z");

            return specimen;
        }

        public static Composition populateImmunizationRecordCompositionResource()
        {
            Composition composition = new Composition()
            {
                Id = "ImmunizationRecommendation-01",
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ImmunizationRecord",
                    },
                },
            };

            // Set logical id of this artifact
            composition.Id = "c26f5e55-3049-4fde-80a4-7e4476be16dd";

            // Set language of the resource content
            composition.Language = "en-IN";
           
            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://ndhm.in/phr";
            identifier.Value = "645bb0c3-ff7e-4123-bef5-3852a4784813";

            // Status can be preliminary | final | amended | entered-in-error
            composition.Status = CompositionStatus.Final;

            // Kind of composition ("Immunization record")
            CodeableConcept type = new CodeableConcept();
            type.Coding.Add(new Coding("http://snomed.info/sct", "41000179103", "Immunization record"));
            type.Text = "Immunization record";
            composition.Type = type;

            // Set subject - Who and/or what the composition/Immunization record is about
            composition.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134", "ABC");

            // Set Timestamp
            composition.DateElement = new FhirDateTime("2020-07-09T15:32:26.605+05:30");

            // Set author - Who and/or what authored the composition/Immunization record
            composition.Author.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147", "Dr DEF"));

            // Set a Human Readable name/title
            composition.Title = "Immunization record";

            // Set Custodian - Organization which maintains the composition
            composition.Custodian = new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24", "UVW Hospital");

            ResourceReference reference1 = new ResourceReference();
            reference1.Reference = "urn:uuid:34403a5b-4ae3-4996-9e76-10e9bc16476e";
            reference1.Type = "Immunization";

            ResourceReference reference2 = new ResourceReference();
            reference2.Reference = "urn:uuid:9fcd0d40-e075-431f-ade5-a2e7b01858cd";
            reference2.Type = "ImmunizationRecommendation";

            ResourceReference reference3 = new ResourceReference();
            reference3.Reference = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";
            reference3.Type = "DocumentReference";

            Composition.SectionComponent section = new Composition.SectionComponent();
            section.Title = "Immunization record";
            section.Code = (new CodeableConcept("http://snomed.info/sct", "41000179103", "Immunization record", "null"));
            section.Entry.Add(reference1);
            section.Entry.Add(reference2);
            section.Entry.Add(reference3);
            composition.Section.Add(section);

            return composition;

        }

        // Populate Immunization Resource
        public static Immunization populateImmunizationResource()
        {
            Immunization immunization = new Immunization()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Immunization",
                    },
                },
            };
            immunization.Id = "34403a5b-4ae3-4996-9e76-10e9bc16476e"; 
             
            immunization.Status = Immunization.ImmunizationStatusCodes.Completed;
            immunization.VaccineCode = new CodeableConcept("http://snomed.info/sct", "1119305005", "COVID-19 antigen vaccine", "COVID-19 antigen vaccine");
            immunization.Patient = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            immunization.Occurrence = new FhirDateTime("2021-02-21");
            immunization.LotNumber = "BSCD12344SS";
            immunization.PrimarySource = true;
            return immunization;
        }

        // Populate Immunization Recommendation Resource
        public static ImmunizationRecommendation populateImmunizationRecommendation()
        {
            ImmunizationRecommendation immunizationRecommendation = new ImmunizationRecommendation();


            immunizationRecommendation.Id = "9fcd0d40-e075-431f-ade5-a2e7b01858cd";
            immunizationRecommendation.Patient = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            immunizationRecommendation.Authority = new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24");
            immunizationRecommendation.DateElement = new FhirDateTime("2021-01-10T11:04:15.817-05:00");
            
            ImmunizationRecommendation.RecommendationComponent immnunizationRecommendationRecommendationComponent = new ImmunizationRecommendation.RecommendationComponent();
            immnunizationRecommendationRecommendationComponent.VaccineCode.Add(new CodeableConcept("http://snomed.info/sct", "1119305005", "COVID-19 antigen vaccine", "COVID-19 antigen vaccine"));
            immnunizationRecommendationRecommendationComponent.ForecastStatus = new CodeableConcept("http://terminology.hl7.org/CodeSystem/immunization-recommendation-status", "due", "Due", "Due");
            immnunizationRecommendationRecommendationComponent.Description = "Second sequence in protocol";
            immnunizationRecommendationRecommendationComponent.Series = "Vaccination Series 2";
            immnunizationRecommendationRecommendationComponent.DoseNumber = new PositiveInt(1);
            immnunizationRecommendationRecommendationComponent.SeriesDoses = new PositiveInt(2);
            immnunizationRecommendationRecommendationComponent.SupportingImmunization.Add(new ResourceReference("urn:uuid:34403a5b-4ae3-4996-9e76-10e9bc16476e"));
            immunizationRecommendation.Recommendation.Add(immnunizationRecommendationRecommendationComponent);

            ImmunizationRecommendation.DateCriterionComponent dateCreationComponet = new ImmunizationRecommendation.DateCriterionComponent();
            dateCreationComponet.Code = new CodeableConcept("http://loinc.org", "30980-7", "Date vaccine due", "Date vaccine due");
            dateCreationComponet.ValueElement = new FhirDateTime("2021-05-10T00:00:00-05:00");
            immnunizationRecommendationRecommendationComponent.DateCriterion.Add(dateCreationComponet);

            return immunizationRecommendation;
        }

        public static Composition populateWellnessRecordCompositionResource()
        {
            Composition composition = new Composition()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/WellnessRecord",
                    },
                },
            };

            // Set logical id of this artifact
            composition.Id = "5268c7b1-5b29-44ec-b25f-f6be91e46511";

            // Set language of the resource content
            composition.Language = "en-IN";
            

            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://ndhm.in/phr";
            identifier.Value = "645bb0c3-ff7e-4123-bef5-3852a4784813";
            composition.Identifier = identifier;

            // Status can be preliminary | final | amended | entered-in-error
            composition.Status = CompositionStatus.Final;

            // Kind of composition ("Wellness record")
            CodeableConcept type = new CodeableConcept();
            //type.Coding.Add(new Coding("http://snomed.info/sct", "41000179103", "Immunisation record"));// "Wellness Record"));
            type.Text = "Wellness Record";
            composition.Type = type;

            // Set subject - Who and/or what the composition/Wellness record is about
            composition.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134", "ABC");

            // Set Timestamp
            composition.DateElement = new FhirDateTime("2020-07-09T15:32:26.605+05:30");

            // Set author - Who and/or what authored the composition/Wellness record
            composition.Author.Add(new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147", "Dr DEF"));

            // Set a Human Readable name/title
            composition.Title = "Wellness Record";


            Composition.SectionComponent section1 = new Composition.SectionComponent();
            section1.Title = "Vital Signs";
            section1.Entry.Add(new ResourceReference("urn:uuid:d28df502-7c86-43a1-a4ec-ecd7f026bd37"));
            section1.Entry.Add(new ResourceReference("urn:uuid:bd809f65-3248-412f-98f4-6d5e38c71833"));
            section1.Entry.Add(new ResourceReference("urn:uuid:a9a3d290-a2c5-4b0c-8d3a-977625273136"));
            section1.Entry.Add(new ResourceReference("urn:uuid:1a100486-f1be-47ba-af73-b86b850f2cea"));
            section1.Entry.Add(new ResourceReference("urn:uuid:b700d75e-d819-45aa-8981-d5941c8abfee"));


            Composition.SectionComponent section2 = new Composition.SectionComponent();
            section2.Title = "Body Measurement";
            section2.Entry.Add(new ResourceReference("urn:uuid:cd3b03d5-e10a-4934-8fe9-37f17bdf458c"));
            section2.Entry.Add(new ResourceReference("urn:uuid:9ef438b3-1b55-44f7-8ae5-879afc7eaafb"));
            section2.Entry.Add(new ResourceReference("urn:uuid:86357581-6eb5-43e3-900a-5a729ab2cd90"));


            Composition.SectionComponent section3 = new Composition.SectionComponent();
            section3.Title = "Physical Activity";
            section3.Entry.Add(new ResourceReference("urn:uuid:42a0955b-b1cb-4727-96d7-b202ff5db03f"));
            section3.Entry.Add(new ResourceReference("urn:uuid:9dd4c4e5-554e-4b05-bfc8-6aed5d258bd3"));
            section3.Entry.Add(new ResourceReference("urn:uuid:dbb4f26a-b2a8-4726-8822-d40a08e67328"));

            Composition.SectionComponent section4 = new Composition.SectionComponent();
            section4.Title = "General Assessment";
            section4.Entry.Add(new ResourceReference("urn:uuid:93ed4e3c-bfd8-4336-aa84-4cea378c655a"));
            section4.Entry.Add(new ResourceReference("urn:uuid:d6878a47-b725-4f91-b90c-1e24f22f09c0"));
            section4.Entry.Add(new ResourceReference("urn:uuid:72776d5e-1a2b-4351-9901-4a7ae5707500"));
            section4.Entry.Add(new ResourceReference("urn:uuid:3cf120f5-bb5f-411c-98fd-1d1d52d399f8"));


            Composition.SectionComponent section5 = new Composition.SectionComponent();
            section5.Title = "Women Health";
            section5.Entry.Add(new ResourceReference("urn:uuid:527524b3-14e1-4a56-a459-e0af928b85bb"));
            section5.Entry.Add(new ResourceReference("urn:uuid:ebde97f8-dd36-4fd2-b8e5-6e3ff12b61dd"));

            Composition.SectionComponent section6 = new Composition.SectionComponent();
            section6.Title = "Lifestyle";
            section6.Entry.Add(new ResourceReference("urn:uuid:e4d2d422-6d86-4e93-9c3d-80190ed709f9"));
            section6.Entry.Add(new ResourceReference("urn:uuid:f11dd16c-37a7-4ded-9288-9f1b806d4911"));

            Composition.SectionComponent section7 = new Composition.SectionComponent();
            section7.Title = "Document Reference";
            section7.Entry.Add(new ResourceReference("urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340"));

            composition.Section.Add(section1);
            composition.Section.Add(section2);
            composition.Section.Add(section3);
            composition.Section.Add(section4);
            composition.Section.Add(section5);
            composition.Section.Add(section6);
            composition.Section.Add(section7);

            return composition;

        }

        // Populate Observation/respiratory-rate Resource
        public static Observation populateRespitaryRateResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationVitalSigns",
                    },
                },
            };
            observation.Id = "d28df502-7c86-43a1-a4ec-ecd7f026bd37";
            
            observation.Status = ObservationStatus.Final;
            observation.Category.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/observation-category", "vital-signs", "Vital Signs", "Vital Signs"));
            observation.Code = new CodeableConcept("http://loinc.org", "9279-1", "Respiratory rate", "Respiratory rate");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            var quantity = new Quantity(44, "breaths/minute", "http://unitsofmeasure.org");
            quantity.Code = "/min";
            observation.Value = quantity;
            observation.Effective = new FhirDateTime("2020-09-29");

            return observation;

        }
        // Populate Observation/heart-rate Resource
        public static Observation populateHeartRateResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationVitalSigns",
                    },
                },
            };
            observation.Id = "bd809f65-3248-412f-98f4-6d5e38c71833";
                        
            observation.Status = ObservationStatus.Final;
            observation.Category.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/observation-category", "vital-signs", "Vital Signs", "Vital Signs"));
            observation.Code = new CodeableConcept("http://loinc.org", "8867-4", "Heart rate", "Heart rate");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Effective = new FhirDateTime("2020-09-29");
            var quantity = new Quantity(44, "beats/minute", "http://unitsofmeasure.org");
            quantity.Code = "/min";
            observation.Value = quantity;
            return observation;
        }

        // Populate Observation/oxygen-saturation Resource
        public static Observation populateOxygenSaturationResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationVitalSigns",
                    },
                },
            };
            observation.Id = "a9a3d290-a2c5-4b0c-8d3a-977625273136";
             
            var identifier = new Identifier("http://goodcare.org/observation/id", "o1223435-10");
            observation.Identifier.Add(identifier);

            observation.Status = ObservationStatus.Final;
            observation.Category.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/observation-category", "vital-signs", "Vital Signs", "Vital Signs"));

            observation.Code = new CodeableConcept("http://loinc.org", "2708-6", "Oxygen saturation in Arterial blood", "Oxygen saturation in Arterial blood");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Effective = new FhirDateTime("2020-09-29T09:30:10+01:00");
            var quantity = new Quantity(95, "%", "http://unitsofmeasure.org");
            quantity.Code = "%";
            observation.Value = quantity;

            observation.Interpretation.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation", "N", "Normal", "Normal (applies to non-numeric results)"));
      ;
            Observation.ReferenceRangeComponent obserrefrange = new Observation.ReferenceRangeComponent();
            quantity = new Quantity(90, "%", "http://unitsofmeasure.org");
            quantity.Code = "%";
            obserrefrange.Low = quantity;

            quantity = new Quantity(95, "%", "http://unitsofmeasure.org");
            quantity.Code = "%";
            obserrefrange.High = quantity;
            observation.ReferenceRange.Add(obserrefrange);

            return observation;
        }

        // Populate "Observation/body-temperature Resource
        public static Observation populateBodyTemperatureResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationVitalSigns",
                    },
                },
            };
            observation.Id = "1a100486-f1be-47ba-af73-b86b850f2cea"; 

            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "61008-9", "Body surface temperature", "Body surface temperature");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Effective = new FhirDateTime("2020-09-29");
            var quantity = new Quantity(36.5m, "Cel", "http://unitsofmeasure.org");
            quantity.Code = "{Cel or degF}";
            observation.Value = quantity;

            return observation;
        }

        // Populate Observation/body-height Resource
        public static Observation populateBodyHeightResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationBodyMeasurement",
                    },
                },
            };
            observation.Id = "cd3b03d5-e10a-4934-8fe9-37f17bdf458c";
             
            observation.Status = ObservationStatus.Final;
            observation.Category.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/observation-category", "vital-signs", "Vital Signs", "Vital Signs"));
            observation.Code = new CodeableConcept("http://loinc.org", "8302-2", "Body height", "Body height");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Effective = new FhirDateTime("2020-09-29");

            var quantity = new Quantity(66.89999999999999m, "in", "http://unitsofmeasure.org");
            quantity.Code = "[in_i]";
            observation.Value = quantity;

            return observation;
        }

        // Populate Observation/body-weight Resource
        public static Observation populateBodyWeightResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationBodyMeasurement",
                    },
                },
            };
            observation.Id = "9ef438b3-1b55-44f7-8ae5-879afc7eaafb";
           
            observation.Status = ObservationStatus.Final;
            observation.Category.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/observation-category", "vital-signs", "Vital Signs", "Vital Signs"));
            observation.Code = new CodeableConcept("http://loinc.org", "29463-7", "Body weight", "Body weight");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Effective = new FhirDateTime("2020-09-29");

            var quantity = new Quantity(185, "lbs", "http://unitsofmeasure.org");
            quantity.Code = "[lb_av]";
            observation.Value = quantity;

            return observation;
        }
        // Populate Observation/body-weight Resource
        public static Observation populateBloodPressureResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationVitalSigns",
                    },
                },
            };
            observation.Id = "b700d75e-d819-45aa-8981-d5941c8abfee";
            
            var identifier = new Identifier();
            identifier.System = "urn:ietf:rfc:3986";
            identifier.Value = "urn:uuid:187e0c12-8dd2-67e2-99b2-bf273c878281";
            observation.Identifier.Add(identifier);
            observation.Status = ObservationStatus.Final;
            observation.Category.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/observation-category", "vital-signs", "Vital Signs"));
            observation.Code = (new CodeableConcept("http://loinc.org", "85354-9", "Blood pressure panel with all children optional", "Blood pressure panel with all children optional"));
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Effective = new FhirDateTime("2020-09-29");
            observation.Performer.Add(new ResourceReference("Practitioner/1"));
            observation.Interpretation.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation", "L", "low", "Below low normal"));
            observation.BodySite = new CodeableConcept("http://snomed.info/sct", "368209003", "Right arm");

            List<Observation.ComponentComponent> componentList = new List<Observation.ComponentComponent>();
            Observation.ComponentComponent component = new Observation.ComponentComponent();
            component.Code = new CodeableConcept("http://loinc.org", "8480-6", "Systolic blood pressure");
            var quantity = new Quantity(107, "mmHg", "http://unitsofmeasure.org");
            quantity.Code = "mm[Hg]";
            component.Value = quantity;
            
            component.Interpretation.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation", "N", "normal", "Normal"));

            Observation.ComponentComponent component1 = new Observation.ComponentComponent();
            component1.Code = new CodeableConcept("http://loinc.org", "8462-4", "Diastolic blood pressure");
            quantity = new Quantity(60, "mmHg", "http://unitsofmeasure.org");
            quantity.Code = "mm[Hg]";
            component1.Value = quantity;
            component1.Interpretation.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation", "L", "low", "Below low normal"));

            componentList.Add(component);
            componentList.Add(component1);
            observation.Component=componentList;

            return observation;
        }
        // Populate Observation/stepCount Resource
        public static Observation populateStepCountResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationPhysicalActivity",
                    },
                },
            };
            observation.Id = "42a0955b-b1cb-4727-96d7-b202ff5db03f";
            
            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "55423-8", "Number of steps in unspecified time Pedometer", "Number of steps in unspecified time Pedometer");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            var quantity = new Quantity(10000, "steps", "http://unitsofmeasure.org");
            quantity.Code = "{steps}";
            observation.Value = quantity;

            return observation;
        }

        // Populate "Observation/CaloriesBurned" Resource
        public static Observation populateCaloriesBurnedResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "CaloriesBurned-1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationPhysicalActivity",
                    },
                },
            };
            observation.Id = "9dd4c4e5-554e-4b05-bfc8-6aed5d258bd3";
            
            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "41981-2", "Calories burned", "Calories burned");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Performer.Add(new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24"));
            var quantity = new Quantity(800, "kcal", "http://unitsofmeasure.org");
            quantity.Code = "kcal";
            observation.Value = quantity;


            return observation;
        }
        // Populate "Observation/SleepDuration" Resource
        public static Observation populateSleepDurationResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "SleepDuration-1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationPhysicalActivity",
                    },
                },
            };
            observation.Id = "dbb4f26a-b2a8-4726-8822-d40a08e67328";
             
            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "93832-4", "Sleep duration", "Sleep duration");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            var quantity = new Quantity(8, "h", "http://unitsofmeasure.org");
            quantity.Code = "h";
            observation.Value = quantity;
            return observation;
        }

        // Populate "Observation/BodyFatMass" Resource
        public static Observation populateBodyFatMassResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationGeneralAssessment",
                    },
                },
            };
            observation.Id = "93ed4e3c-bfd8-4336-aa84-4cea378c655a";            
            
            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "73708-0", "Body fat [Mass] Calculated", "Body fat [Mass] Calculated");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Performer.Add(new ResourceReference("urn:uuid:b9768f37-82cb-471e-934f-71b9ce233656"));
            var quantity = new Quantity(11, "kg", "http://unitsofmeasure.org");
            quantity.Code = "kg";
            observation.Value = quantity;

            return observation;
        }
        // Populate "Observation/BloodGlucose" Resource
        public static Observation populateBloodGlucoseResource()
        {
            Observation observation = new Observation()
            {  // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationGeneralAssessment",
                    },
                },
            };
            observation.Id = "d6878a47-b725-4f91-b90c-1e24f22f09c0";
       

            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "2339-0", "Glucose [Mass/volume] in Blood", "Glucose [Mass/volume] in Blood");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            var quantity = new Quantity(142, "mg/dL", "http://unitsofmeasure.org");
            quantity.Code = "mg/dL";
            observation.Value = quantity;

            return observation;
        }
        // Populate "Observation/FluidIntake" Resource
        public static Observation populateFluidIntakeResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationGeneralAssessment",
                    },
                },
            };
            observation.Id = "72776d5e-1a2b-4351-9901-4a7ae5707500";
          

            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "8999-5", "Fluid intake oral Estimated", "Fluid intake oral Estimated");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            var quantity = new Quantity(3, "Litres", "http://unitsofmeasure.org");
            quantity.Code = "{mL or Litres}";
            observation.Value = quantity;

            return observation;
        }
        // Populate "Observation/CalorieIntake" Resource
        public static Observation populateCalorieIntakeResource()
        {
            Observation observation = new Observation()
            {  // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationGeneralAssessment",
                    },
                },
            };
            observation.Id = "3cf120f5-bb5f-411c-98fd-1d1d52d399f8";
     

            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "9052-2", "Calorie intake total", "Calorie intake total");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            var quantity = new Quantity(1750, "kcal", "http://unitsofmeasure.org");
            quantity.Code = "kcal";
            observation.Value = quantity;

            return observation;
        }
        // Populate "Observation/AgeOfMenarche" Resource
        public static Observation populateAgeOfMenarcheResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationWomenHealth",
                    },
                },
            };
            observation.Id = "527524b3-14e1-4a56-a459-e0af928b85bb";
          
            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "42798-9", "Age at menarche", "Age at menarche");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            var quantity = new Quantity(14, "age", "http://unitsofmeasure.org");
            quantity.Code = "{age}";
            observation.Value = quantity;

            return observation;
        }
        // Populate "Observation/LastMenstrualPeriod" Resource
        public static Observation populateLastMenstrualPeriodResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationWomenHealth",
                    },
                },
            };
            observation.Id = "ebde97f8-dd36-4fd2-b8e5-6e3ff12b61dd";
           

            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "8665-2", "Last menstrual period start date", "Last menstrual period start date");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Effective = new FhirDateTime("2020-11-14");
            var quantity = new Quantity(110120, "MMDDYY", "http://unitsofmeasure.org");
            quantity.Code = "{MMDDYY}";
            observation.Value = quantity;

            return observation;
        }
        // Populate "Observation/DietType" Resource
        public static Observation populateDietTypeResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationLifestyle",
                    },
                },
            };
            observation.Id = "e4d2d422-6d86-4e93-9c3d-80190ed709f9";
             
            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://loinc.org", "81663-7", "Diet [Type]", "Diet [Type]");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Value = new CodeableConcept("http://snomed.info/sct", "138045004", "Vegan diet", "Vegan diet");

            return observation;
        }
        // Populate "Observation/TobaccoSmokingStatus" Resource
        public static Observation populateTobaccoSmokingStatusResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationLifestyle",
                    },
                },
            };
            observation.Id = "f11dd16c-37a7-4ded-9288-9f1b806d4911";
             
            observation.Status = ObservationStatus.Final;
            observation.Code = new CodeableConcept("http://snomed.info/sct", "365981007", "Finding of tobacco smoking behavior", "Finding of tobacco smoking behavior");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Value = new CodeableConcept("http://snomed.info/sct", "266919005", "Never smoked tobacco", "Never smoked tobacco");

            return observation;
        }

        // Populate Observation/bmi Resource
        public static Observation populateBMIResource()
        {
            Observation observation = new Observation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ObservationBodyMeasurement",
                    },
                },
            };
            observation.Id = "86357581-6eb5-43e3-900a-5a729ab2cd90";
           

            observation.Status = ObservationStatus.Final;
            observation.Category.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/observation-category", "vital-signs", "Vital Signs", "vital Signs"));
            observation.Code = new CodeableConcept("http://loinc.org", "39156-5", "Body mass index (BMI) [Ratio]", "BMI");
            observation.Subject = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            observation.Effective = new FhirDateTime("2020-09-29");
            var quantity = new Quantity(16.2m, "kg/m2", "http://unitsofmeasure.org");
            quantity.Code = "kg/m2";
            observation.Value = quantity;
            return observation;
        }


        // Populate Coverage Resource
        public static Coverage populateCoverageResource()
        {
            // set Meta - Metadata about the resource
            Coverage coverage = new Coverage()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Coverage",
                    },
                },
            };

            coverage.Id = "Coverage-01";             

            coverage.Identifier.Add(new Identifier("http://hospitalx.com/selfpayagreement", "SP12345678"));
            coverage.Status = FinancialResourceStatusCodes.Active;
            coverage.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/coverage-selfpay", "pay", "PAY");
            coverage.Subscriber = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            coverage.Beneficiary = new ResourceReference("urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134");
            coverage.Relationship = new CodeableConcept("http://terminology.hl7.org/CodeSystem/subscriber-relationship", "self", "SELF");
            coverage.Period = new Period(new FhirDateTime("2020-04-20T15:32:26.605+05:30"), new FhirDateTime("2020-05-01T15:32:26.605+05:30"));
            coverage.Payor.Add(new ResourceReference("urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24"));

            return coverage;
        }


        // Populate Signature Resource
        public static Signature populateSignature()
        {
            Signature signature = new Signature();
            Coding item1 = new Coding();
            item1.System = "urn:iso-astm:E1762-95:2013";
            item1.Code = "1.2.840.10065.1.12.1.1";
            item1.Display = "Author's Signature";
            signature.Type.Add(item1);
            signature.When = new DateTime(2020, 07, 09, 07, 42, 33);
            signature.Who = new ResourceReference("urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147");
            signature.SigFormat = "image/jpeg";
            string data = "R0lGODlhfgCRAPcAAAAAAIAAAACAAICAAAAAgIAA oxrXyMY2uvGNcIyj    HOeoxkXBh44OOZdn8Ggu+DiPjwtJ2CZyUomCTRGO";
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            signature.Data = bytes;

            return signature;
        }

        //Populate Medication Resource
        public static Medication populateMedicationResource()
        {
            Medication medication = new Medication()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Medication",
                    },
                },
            };
            medication.Id = "Medication-01";
            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.Value = "29222933";
            identifier.Type = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-identifier-type-code", "HSN", "Harmonized System of Nomenclature", "Harmonized System of Nomenclature");
            medication.Identifier.Add(identifier);

            medication.Code = new CodeableConcept("http://snomed.info/sct", "1172863005", "Paracetamol 1 g oral tablet", "Paracetamol 1 g oral tablet");
            medication.Form = new CodeableConcept("http://snomed.info/sct", "385055001", "Tablet", "Tablet");

            Medication.BatchComponent batchcomponent = new Medication.BatchComponent();
            batchcomponent.LotNumber = "22180423";
            batchcomponent.ExpirationDate = "2023-02-24";

            medication.Batch = batchcomponent;

            return medication;
        }

    }

}
