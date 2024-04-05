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
using Task = Hl7.Fhir.Model.Task;


namespace FHIR_NHCX
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
                        //Reading Existing or created Json Bundle into Base Profile
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
                    OperationOutcome outcome = validator.Validate(ProfileInstance);
                    
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
        //Patient/Patient-01
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
                },
            };

            patient.Id = "1efe03bf-9506-40ba-bc9a-80b0d5045afe";          

            var id = new Identifier();
            id.System = "https://uidai.gov.in/";
            id.Value = "7225-4829-5255";
            id.Type = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-identifier-type-code", "ADN", "Adhaar number","Adhaar number");
            patient.Identifier.Add(id);

            var name = new HumanName();
            name.Text = "Ayush Sharma";
            patient.Name.Add(name);

            List<ContactPoint> list = new List<ContactPoint>();
            ContactPoint contact1 = new ContactPoint();
            contact1.System = ContactPoint.ContactPointSystem.Phone;
            contact1.Value = "+919818512600";
            contact1.Use = ContactPoint.ContactPointUse.Home;


            patient.Gender = AdministrativeGender.Male;
            patient.BirthDate = "1981-01-12";

            return patient;                      
             
        }

        // populate Second patient
        public static Patient populateSecondPatientResource()
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
                },
            };

            patient.Id = "Patient-02";

            

            var id = new Identifier();
            id.System = "https://uidai.gov.in/";
            id.Value = "7225-4829-555";
            id.Type = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-identifier-type-code", "ADN", "Adhaar number");
            patient.Identifier.Add(id);

            var name = new HumanName();
            name.Text = "Samer Sharma";
            patient.Name.Add(name);

            List<ContactPoint> list = new List<ContactPoint>();
            ContactPoint contact1 = new ContactPoint();
            contact1.System = ContactPoint.ContactPointSystem.Phone;
            contact1.Value = "+919818512601";
            contact1.Use = ContactPoint.ContactPointUse.Home;


            patient.Gender = AdministrativeGender.Male;
            patient.BirthDate = "1981-01-12";

            return patient;

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
            practitioner.Id = "3bc96820-c7c9-4f59-900d-6b0ed1fa558e";                  
           

            var coding = new List<Coding>();
            coding.Add(new Coding("http://terminology.hl7.org/CodeSystem/v2-0203", "MD", "Medical License number"));
            Identifier identifier = new Identifier();
            identifier.System = "https://ndhm.in/DigiDoc";
            identifier.Value = "7601003178999";
            identifier.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "MD", "Medical License number");
            identifier.Type.Coding = coding;
            practitioner.Identifier.Add(identifier);
            var name = new HumanName();
            name.Text = "Dr. DEF";
            practitioner.Name.Add(name);
            return practitioner;
        }

        // Populate Procedure Resource
        public static Procedure populateProcedureResource()
        {
            Procedure procedure = new Procedure()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                       "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Procedure",
                    },
                }
            };
            procedure.Id ="Procedure-01";

            procedure.Status = EventStatus.Completed;
            procedure.Code = (new CodeableConcept("http://snomed.info/sct", "36969009", "Placement of stent in coronary artery","Placement of stent in coronary artery"));
            procedure.Subject = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");

            procedure.Performed = new FhirDateTime(2019,05,12);
            procedure.Complication.Add(new CodeableConcept("http://snomed.info/sct", "131148009", "Bleeding"));
            return procedure;
        }

        // Populate Organization Resource 
        public static Organization populateOrganizationResource()
        {
            // Set logical id of this artifact
            Organization organization = new Organization()            {
                
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
            organization.Id ="3a947161-4033-45d1-8b9c-7e9115c6000f";
        
            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://facility.ndhm.gov.in";
            identifier.Value = "4567878";
            identifier.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "PRN", "Provider number", "Provider number");
            organization.Identifier.Add(identifier);
            organization.Name = "XYZ insurance pvt. ltd.";
           
           
            List<ContactPoint> list = new List<ContactPoint>();
            ContactPoint contact1 = new ContactPoint();
            contact1.System = ContactPoint.ContactPointSystem.Phone;
            contact1.Value = "+91 243 2634 1234";
            contact1.Use = ContactPoint.ContactPointUse.Work;

            ContactPoint contact2 = new ContactPoint();
            contact2.System = ContactPoint.ContactPointSystem.Email;
            contact2.Value = "contact@insurance.xyz.org";
            contact2.Use = ContactPoint.ContactPointUse.Work;

            list.Add(contact1);
            list.Add(contact2);
            organization.Telecom = list; 

            return organization;
        }

        // Populate Second Organization Resource
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

            organization.Id = "36482e82-ef80-4995-8acb-faed97be573f";           
            // Set version-independent identifier for the Composition
            Identifier identifier = new Identifier();
            identifier.System = "https://facility.ndhm.gov.in";
            identifier.Value = "4567878";
            identifier.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "PRN", "Provider number", "Provider number");
            organization.Identifier.Add(identifier);
            organization.Name = "Aditya Birla Health Insurance";           

            List<ContactPoint> list = new List<ContactPoint>();
            ContactPoint contact1 = new ContactPoint();
            contact1.System = ContactPoint.ContactPointSystem.Phone;
            contact1.Value = "+91 243 2634 1234";
            contact1.Use = ContactPoint.ContactPointUse.Work;

            ContactPoint contact2 = new ContactPoint();
            contact2.System = ContactPoint.ContactPointSystem.Email;
            contact2.Value = "contact@insurance.xyz.org";
            contact2.Use = ContactPoint.ContactPointUse.Work;

            list.Add(contact1);
            list.Add(contact2);
            organization.Telecom = list;

            return organization;
        }


        // Populate Hospital Organization Resource 
        public static Organization populateHospitalOrganizationResource()
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


            organization.Id = "4a88bdc0-d320-4138-8014-d41913d9745a";
            // Set version-independent identifier for the Composition

            Identifier identifier = new Identifier();
            identifier.System = "https://facility.ndhm.gov.in";
            identifier.Value = "4567878";
            identifier.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/organization-type", "prov", "Healthcare Provider", "Healthcare Provider");
            organization.Identifier.Add(identifier);

            organization.Name ="XYZ Hospital Co. Ltd.";          

            List<ContactPoint> list = new List<ContactPoint>();
            ContactPoint contact1 = new ContactPoint();
            contact1.System = ContactPoint.ContactPointSystem.Phone;
            contact1.Value = "+91 243 2634 1234";
            contact1.Use = ContactPoint.ContactPointUse.Work;

            ContactPoint contact2 = new ContactPoint();
            contact2.System = ContactPoint.ContactPointSystem.Email;
            contact2.Value = "contact@hospital.xyz.org";
            contact2.Use = ContactPoint.ContactPointUse.Work;

            list.Add(contact1);
            list.Add(contact2);
            organization.Telecom = list;

            return organization;
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

            coverage.Id = "0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5";                     

            coverage.Identifier.Add(new Identifier("http://hospitalx.com/selfpayagreement", "SP12345678"));
            coverage.Status = FinancialResourceStatusCodes.Active;
            coverage.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/coverage-selfpay", "pay", "PAY");
            coverage.Subscriber = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");
            coverage.SubscriberId ="ABC123456BI007";
            coverage.Beneficiary = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");
            coverage.Relationship = new CodeableConcept("http://terminology.hl7.org/CodeSystem/subscriber-relationship", "self", "SELF");
            coverage.Period = new Period(new FhirDateTime("2020-04-20T15:32:26.605+05:30"), new FhirDateTime("2020-05-01T15:32:26.605+05:30"));
            coverage.Payor.Add(new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f"));

            return coverage;
        }

        // Populate Location Resource
        public static Location populateLocationResource()
        {
            // set Meta - Metadata about the resource
            Location location = new Location();            

            location.Id = "1cb884ad-0df4-4c35-ae40-4764895c84c6";

            location.Identifier.Add(new Identifier("https://irdai.gov.in", "B1-S.F2"));
            location.Status = Location.LocationStatus.Active;
            location.Name = "South Wing, second floor";
            IEnumerable<string> alias = new string[] { "IndiaFirst Life Insurance Co. Ltd., South Wing, second floor" };
            location.Alias = alias;
            location.Description = "Second floor of the Old South Wing, formerly in use by Psychiatry";
            location.Mode = Location.LocationMode.Instance;

            List<ContactPoint> list = new List<ContactPoint>();
            ContactPoint contact1 = new ContactPoint();
            contact1.System = ContactPoint.ContactPointSystem.Phone;
            contact1.Value = "2328";
            contact1.Use = ContactPoint.ContactPointUse.Work;

            Address contact2 = new Address();
            contact2.Use = Address.AddressUse.Work;
            contact2.Line = new string[] { "91, Building A" };
            contact2.City = "Pune";
            contact2.PostalCode = "451855";
            contact2.Country = "IND";

            list.Add(contact1);

            location.Telecom = list;
            location.Address = contact2;

            location.PhysicalType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/location-physical-type", "wi", "Wing");
            Location.PositionComponent positioncomponant = new Location.PositionComponent();
            positioncomponant.Longitude = (decimal)-83.6945691;
            positioncomponant.Latitude = (decimal)42.25475478;
            positioncomponant.Altitude = 0;
            location.Position = positioncomponant;

            location.ManagingOrganization = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            return location;
        }

        // Populate Medication Request Resource
        public static MedicationRequest populateMedicationRequestResource()
        {
            MedicationRequest medicationRequest = new MedicationRequest()
            {

                Meta = new Meta()
                {
                    VersionId = "1",                 
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/MedicationRequest",
                    },

                },
            };
            medicationRequest.Id ="b8970fa7-ae4a-47b4-9405-e382b3f7c055";
            medicationRequest.Status = MedicationRequest.medicationrequestStatus.Active;
            medicationRequest.Intent = MedicationRequest.medicationRequestIntent.Order;

            medicationRequest.Medication = new CodeableConcept("http://snomed.info/sct", "765507008", "Metformin hydrochloride 500 mg prolonged-release oral tablet", "Metformin hydrochloride 500 mg prolonged-release oral tablet");

            medicationRequest.Subject = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe", "ABC");

            medicationRequest.AuthoredOnElement = new FhirDateTime("2020-07-09");
            
            medicationRequest.Requester = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e", "Dr DEF");

            medicationRequest.ReasonCode.Add(new CodeableConcept("http://snomed.info/sct", "44054006", "Type 2 diabetes mellitus", "Type 2 diabetes mellitus"));

            medicationRequest.ReasonReference.Add(new ResourceReference("urn:uuid:bdaebfe7-8296-4241-9629-b16c364a10b4"));

            List<Dosage> dosage1 = new List<Dosage>();
            var objdosage = new Dosage();
            objdosage.Text = "One tablet at once";

            Timing objTiming = new Timing();
            var objTimeRepeat = new Timing.RepeatComponent();
            objTimeRepeat.Frequency = 1;
            objTimeRepeat.Period = 1;     

            objdosage.AdditionalInstruction.Add(new CodeableConcept("http://snomed.info/sct", "311504000", "With or after food", "With or after food"));
            medicationRequest.DosageInstruction = dosage1;
            objdosage.Route = new CodeableConcept("http://snomed.info/sct", "26643006", "Oral Route", "Oral Route");
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
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/MedicationRequest",
                    },

                },
            };
            medicationRequest.Id = "acefdfbd-e612-483e-90fc-a5c44d09a4b9";
           
            medicationRequest.Status = MedicationRequest.medicationrequestStatus.Active;
            medicationRequest.Intent = MedicationRequest.medicationRequestIntent.Order;

            medicationRequest.Medication = new CodeableConcept("http://snomed.info/sct", "319775004", "Aspirin 75 mg oral tablet", "Aspirin 75 mg oral tablet");

            medicationRequest.Subject = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe", "ABC");

            medicationRequest.AuthoredOnElement = new FhirDateTime("2020-07-09");

            medicationRequest.Requester = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e", "Dr DEF");

            medicationRequest.ReasonCode.Add(new CodeableConcept("http://snomed.info/sct", "410429000", "Cardiac arrest","Cardiac arrest"));

            medicationRequest.ReasonReference.Add(new ResourceReference("urn:uuid:bdaebfe7-8296-4241-9629-b16c364a10b4"));

            List<Dosage> dosage1 = new List<Dosage>();
            var objdosage = new Dosage();
            objdosage.Text = "One tablet at once";

            Timing objTiming = new Timing();
            var objTimeRepeat = new Timing.RepeatComponent();
            objTimeRepeat.Frequency = 1;
            objTimeRepeat.Period = 1;

            objdosage.AdditionalInstruction.Add(new CodeableConcept("http://snomed.info/sct", "311504000", "With or after food", "With or after food"));
            medicationRequest.DosageInstruction = dosage1;
            objdosage.Route = new CodeableConcept("http://snomed.info/sct", "26643006", "Oral Route", "Oral Route");
            objdosage.Method = new CodeableConcept("http://snomed.info/sct", "421521009", "Swallow", "Swallow");

            dosage1.Add(objdosage);

            medicationRequest.DosageInstruction = dosage1;

            return medicationRequest;
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

            condition.Id = "bdaebfe7-8296-4241-9629-b16c364a10b4";         

            condition.Subject = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");

            condition.Code = new CodeableConcept("http://snomed.info/sct", "410429000", "Cardiac arrest","Cardiac arrest");
            condition.Onset = new FhirString("1days");
            condition.Recorder = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e");

            return condition;

        }

        // Populate Document Reference Resource
        public static DocumentReference populateDocumentReferenceResource()
        {
            DocumentReference documentReference = new DocumentReference()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),//new Instant("2020-07-09T15:32:26.605+05:30"),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DocumentReference",
                    },
                },
            };
            documentReference.Id ="e53fa5db-f676-4b16-a273-f4088866314e";
            documentReference.Status = DocumentReferenceStatus.Current;
            documentReference.DocStatus = CompositionStatus.Final;
            documentReference.Subject = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");
            documentReference.Type = new CodeableConcept("http://snomed.info/sct", "4241000179101", "Laboratory report", "Laboratory report");

            var DocumentReferenceContentComponent = new DocumentReference.ContentComponent();
            DocumentReferenceContentComponent.Attachment = new Attachment();
            DocumentReferenceContentComponent.Attachment.ContentType = "application/pdf";
            DocumentReferenceContentComponent.Attachment.Language = "en-IN";
            DocumentReferenceContentComponent.Attachment.Title = "Laboratory report1";
            DocumentReferenceContentComponent.Attachment.CreationElement = new FhirDateTime("2019-05-29T14:58:58.181+05:30");

            byte[] bytes = Encoding.Default.GetBytes("JVBERi0xLjMKMyAwIG9iago8PC9UeXBlIC9QYWdlCi9QYXJlbnQgMSAwIFIKL01lZGlhQm94IFswIDAgODQxLjg5IDU5NS4yOF0KL1Jlc291cmNlcyAyIDAgUgovQ29udGVudHMgNCAwIFI+PgplbmRvYmoKNCAwIG9iago8PC9GaWx0ZXIgL0ZsYXRlRGVjb2RlIC9MZW5ndGggMjI3MD4+CnN0cmVhbQp4nH2ZyXLbSBKG736KjDlNR4hw7YtPDS6W2BYXk9Ri3dgyrOZYXIak3OE59TvMeV6un2QSqIJQKhJ1scMy/j8T+WVlFUoMfntHMqnhz3fdBbz/SIGyjBBYfIPBovwREzxTGqRUGROw+Ar/7BWbY7GHb9s99IsfxfN2t8afwPYb5F9/LDePxVfobde7l+Nq8/QLLP7ljThTmbUgBfrrymi0RJ/V8hmGh8MLqq5Xh2MooBnF55nOLKmeH6x3z9ufRQHj7YfguSpp0iRNMo3vQzJjDf5pjYH90ztmMi5BUplZCpiCZNChIqMa9gXM4d9A4AnKoDxjDCSRmWEuydUB4/lw8PmdRmNdGymWcXXeyOBrytjoK75xY0XLYPw1Kasy0mLGJc+0Ct36xeFxv9odV9tNY6iYzZitDd0rn/VTXGaUh36fjz8DH6kzJWofJK9si4/SGSWhT77evmyOQblQxl/f0RXvrJVG3OJNSrNivdx/b6zwQYebvcHtyRKaaV6TdX+VAZpGEpZmSlbOlBFdN5Dn6eSe51u5xtqTUE5Uh5gOI8TWJjVJn4Qn+daGclU2fuCTL+6ht/x9tSmO8Ofq+AfMR9P5BQxxeT3DVMCvwMraXV79BxcU2jDov2xh/n4D+UOva5kh5OONgfdwbQTtKiMvQMBlF/r9GSsfC9ZIHVzrTLjFJxgjRlLoADfCcqzBeyCCUfzXBVD7D7gdDu7mk/GwV0WcUU0stYJwSS/wRUkZaJ4vcrjq96sn1O2I39/PehcAF2ciK55xt4xHc5hMF8Nefl3pLDpLIqRiRPBaWHeyK6jv5Lf1VJyVPRxifVX7/nVq37+RWpFMmFAtBJfqtSt82zoH37aBg8tISMzyzDgJmk7YMsnSn4vAvjL08pMhEjRdI2e8Qyg2HZVR09VJnIyPsPqNz+V8BKPt12INBnbb/RHmL7vq79Hye4HdcLf8UTxu19Uzz/ABPjNBVBciMD7oyYgJwTQxYzBefTJYQjCB2hJCIjDe4WSe1GCEzIhITgMuMuvAc0t4BMbJU9OgkRPRwYGAYEQMxieRnAaNT7/4fjhud7DA9UBwZ33GGbAvHlcHHPHIarrfApauuzrWE6KaCUPdwUVNyv9+LA4H3JNNuTJn+QhXMSy65fK8cOtzMbv+TNmZpclwxzSuycTff/0Prgd9F3+03ayO2/0ZCUF0tJKU1r1xh7Avk9GXjhZM0Y4Q4w6LO8ZVI7mUm2KcdIxTJ5dyo9baypOOcQ6tS5lUP/Ed45562zHcsnKD8h0T7R9e7jvmrdx1TCCnqq1jfBK+Y97a+NIHPu6UhUexw8/DERc0ngm+Ywed4uLaZExVmqtp/RgYjn3mWmmoYffaQDBd4jgYbzO4nAyYnOZnDJXIiHX891n56HCcC8Yf2NXszNO4lq3rFt9R8Pdf/4XFR3zCZLJsufrnr6EH8sqw/FxoPLpoEYZWvVHca66OvtfeltH3Wogj7jWn9r0WqV2vBWppcL3GveYcfK8FDi4jzqtNJ7FtcEbLM3m1LemglSsnL09sG4Gc2A7hZa+ZqNfqJFLbRuCDY+QDzIvlE55gQeJc6S4fv7/sYPr8coCbebcaMePcfiQPRkY4fKjUZhFEinF4dWqzCNWMnSx979C2WXCqq/mRwEF0xl27K0pkhMPJUzgaOaMdyhAHozEOn0QSR+BTjvbX49co/zSAO3cMu7sfsJzTcX/x6T7m4GIkOTQhTjg4dZJDozbKnmBwBq0YCJ4Qk4cpZquPsyo5YU00gr0+wSHQM9y02bkjfJ1FikPggwfkL3CbDydwezmG7gOVfAzX+XQxmcbFd8ap4ofvFxffqVPFD9RKnJ6YvENb9Zmpzlup6mtbftFW2Umi6dvqe32q+oGedog8V/06i2T1G5/uYLAYXMOieC52f2w3eBraHI77l+o6wp1fyxUhCB4gGVXK0IiJD5dkEmQdMfHqJJNGLayMiHh9KxGN5xmVJIK7sPFjT1EWE3H6FJFGj3sErb4uSEzEZ5Ek0vhclp+T5qbaES6nszmMJv3BqMLAJaOUYmNqw7UiMQoXJ4kieN0YhVMnUTRqzlW8OrxBKwv00DbJQpKypI6FVtEe4fUpFqHef+mxmIXPIsmi8WlmkxtIFQv85s47eBYsiTAthaZG4Je/LrnYk/XhAiahBHnHUJw6CSVQYzYnQ8tbtGIR6M2SWLAqsj51a2siLE6fwtLocYlUW8YpFp9FEkvjU17y4Ewid27bvh1OKxjdGTW4PGSJnnHNCaXMxjxcpCSP4IVjHk6d5NGolT6ZV07fCgMTECIJg1WFdjDw2BzBcPoUjEDfCsNnkYTR+Nzlt4PeZATlrcjJvOKmxKGFotKqGIWLk0QRpBujcOokikYtzswrZ9DKAr+rmE6yICajpmbBogsQr0+xCPS6jYXPIsmi8RnN8QNicZPPrmEwu5zAp8GX7iSf9cvFQuBuOMZzfkVGK4MbCEc21oqYi4uZ5BKkHnNx6iSXRi3ODCxn0MqFiPK+PMGFWvyypq1cnD7BJdS3cvFZpLgEPsgFl8g07y1e721Hk5v5oLoFrm5wGSZKkNJ8buNrHx8rxSNMOebh1CkegVpJGuNw+jYcFE9gNrl/UDzAGeHLKWyEw+tTOBo9959+lEc46iySOBqf2eB2cn07HF9C7yofliNr0B/ejKCb9z7BfAbjLb52RMGHSFII3jSi4NVJCo1am5Odwxu0YtAy08n7EKpkplQrBqdPYWj07Rh8FkkMjU8KQ1x955ysfvCCcfWdOln9Rn2u+s6gtfqKZjJ5/UElzYS/dyDSRNdRXp+qfqD3393spPo+i2T1G5/qenrtLw3LGfS46RB+Px5ddRSlhnSo/q1DH2bzGIULk0TRRDlB4dRJFI063hu8+pTD+V+81wOoJvv62+/56mmzPL7s698kVxXSrFxhzdP92ejsgxzjKhk8eLnfvuxgXjzui2Ox/3lWJCl3Q6gW9bbr9ep4LGBUrH8v9mdFGh+3tvxVrb+amy6fCqD4sfFa1v8D7OEY0wplbmRzdHJlYW0KZW5kb2JqCjEgMCBvYmoKPDwvVHlwZSAvUGFnZXMKL0tpZHMgWzMgMCBSIF0KL0NvdW50IDEKL01lZGlhQm94IFswIDAgNTk1LjI4IDg0MS44OV0KPj4KZW5kb2JqCjUgMCBvYmoKPDwvRmlsdGVyIC9GbGF0ZURlY29kZSAvTGVuZ3RoIDM2ND4+CnN0cmVhbQp4nF1Sy26DMBC88xU+pocITBqCJYRESZA49KHSfgCBJUUqBhly4O+7u3bSqkhY47FndlZrPy+Ppe4X4b+ZsalgEV2vWwPzeDUNiDNceu3JULR9s7gdr81QT56P4mqdFxhK3Y1ekvjveDYvZhWbrB3P8OD5r6YF0+uL2HzmFe6r6zR9wwB6EYGXpqKFDn2e6+mlHkD4LNuWLZ73y7pFze+Nj3UCEfJe2izN2MI81Q2YWl/AS4IgFUlRpB7o9t9ZZBXn7u/VQ4FLgF/qJXGEOD7gEgYhEUoiViETMiZiR8SjJXIiSKKsRO6QyFx9MkVMMW4FpboFaL5qg+UClmXkE7siGWEqEkisi9jVOhHe22QR4ZjuhDljxfyOW8hYGzF+srwinDO/Z88T48OR8jtP4pX1PHJf7Ckt7zwlYedJOZXzpLaV86ScqrA4dt1ztzQOejD3OTdXY3DE/Kp4tjTVXsP94U3jRCr6fwB/DbapCmVuZHN0cmVhbQplbmRvYmoKNiAwIG9iago8PC9UeXBlIC9Gb250Ci9CYXNlRm9udCAvSGVsdmV0aWNhLUJvbGQKL1N1YnR5cGUgL1R5cGUxCi9FbmNvZGluZyAvV2luQW5zaUVuY29kaW5nCi9Ub1VuaWNvZGUgNSAwIFIKPj4KZW5kb2JqCjcgMCBvYmoKPDwvVHlwZSAvRm9udAovQmFzZUZvbnQgL0hlbHZldGljYQovU3VidHlwZSAvVHlwZTEKL0VuY29kaW5nIC9XaW5BbnNpRW5jb2RpbmcKL1RvVW5pY29kZSA1IDAgUgo+PgplbmRvYmoKMiAwIG9iago8PAovUHJvY1NldCBbL1BERiAvVGV4dCAvSW1hZ2VCIC9JbWFnZUMgL0ltYWdlSV0KL0ZvbnQgPDwKL0YxIDYgMCBSCi9GMiA3IDAgUgo+PgovWE9iamVjdCA8PAo+Pgo+PgplbmRvYmoKOCAwIG9iago8PAovUHJvZHVjZXIgKEZQREYgMS44MikKL0NyZWF0aW9uRGF0ZSAoRDoyMDIzMDcyNjA4MDYyNCkKPj4KZW5kb2JqCjkgMCBvYmoKPDwKL1R5cGUgL0NhdGFsb2cKL1BhZ2VzIDEgMCBSCj4+CmVuZG9iagp4cmVmCjAgMTAKMDAwMDAwMDAwMCA2NTUzNSBmIAowMDAwMDAyNDU4IDAwMDAwIG4gCjAwMDAwMDMyMTAgMDAwMDAgbiAKMDAwMDAwMDAwOSAwMDAwMCBuIAowMDAwMDAwMTE3IDAwMDAwIG4gCjAwMDAwMDI1NDUgMDAwMDAgbiAKMDAwMDAwMjk3OSAwMDAwMCBuIAowMDAwMDAzMDk3IDAwMDAwIG4gCjAwMDAwMDMzMjQgMDAwMDAgbiAKMDAwMDAwMzQwMCAwMDAwMCBuIAp0cmFpbGVyCjw8Ci9TaXplIDEwCi9Sb290IDkgMCBSCi9JbmZvIDggMCBSCj4+CnN0YXJ0eHJlZgozNDQ5CiUlRU9G");
            DocumentReferenceContentComponent.Attachment.DataElement = new Hl7.Fhir.Model.Base64Binary(bytes);            
            documentReference.Content.Add(DocumentReferenceContentComponent);

            return documentReference;
        }

        // Populate Document Reference Resource
        public static DocumentReference populateSecondDocumentReferenceResource()
        {
            DocumentReference documentReference = new DocumentReference()
            {
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),//new Instant("2020-07-09T15:32:26.605+05:30"),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DocumentReference",
                    },
                },
            };
            documentReference.Id = "514bcad3-7bf0-43a0-b566-e8ecd815dc91";
            documentReference.Status = DocumentReferenceStatus.Current;
            documentReference.DocStatus = CompositionStatus.Final;
            documentReference.Subject = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");
            documentReference.Type = new CodeableConcept("http://snomed.info/sct", "4241000179101", "Laboratory report", "Laboratory report");

            var DocumentReferenceContentComponent = new DocumentReference.ContentComponent();
            DocumentReferenceContentComponent.Attachment = new Attachment();
            DocumentReferenceContentComponent.Attachment.ContentType = "application/pdf";
            DocumentReferenceContentComponent.Attachment.Language = "en-IN";
            DocumentReferenceContentComponent.Attachment.Title = "Adhar Card PDF";
            DocumentReferenceContentComponent.Attachment.CreationElement = new FhirDateTime("2019-05-29T14:58:58.181+05:30");

            byte[] bytes = Encoding.ASCII.GetBytes("IDc4NTkxPj4NCnN0YXJ0eHJlZg0KODA2MTQNCiUlRU9G");
            DocumentReferenceContentComponent.Attachment.DataElement = new Hl7.Fhir.Model.Base64Binary(bytes);
            documentReference.Content.Add(DocumentReferenceContentComponent);

            return documentReference;
        }

        #region NHCX
        //Populating InsurancePlan resource
        public static InsurancePlan populateInsurancePlan()
        {

            InsurancePlan insurancePlan = new InsurancePlan()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/InsurancePlan",
                    },
                },
            };                      
            
            // set id - Logical id of this artifact
            insurancePlan.Id = "859d47c1-fd1a-45e4-b725-22e0a1e0b84c";

            // set identifer - Business Identifier that should be as provided by IRDAI for Indian insurance.
            Identifier identifier = new Identifier();
            identifier.System = "https://irdai.gov.in";
            identifier.Value = "761234556546";
            insurancePlan.Identifier.Add(identifier);                    


            // set status - ACTIVE
            insurancePlan.Status = PublicationStatus.Active;
            
            // set type - Kind of product
            insurancePlan.Type.Add(new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-insuranceplan-type", "01", "Hospitalisation Indemnity Policy", "Hospitalisation Indemnity Policy"));

            // set name - Official name
            insurancePlan.Name = "Active Assure";


            // set period - When the product is available
            insurancePlan.Period = new Period(new FhirDateTime("2023-09-10"), new FhirDateTime("2024-09-10"));

            // set ownerdBy - Plan issuer
            insurancePlan.OwnedBy = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set adminstraterBy - Product administrator
            insurancePlan.AdministeredBy = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set extension 1 : category :Proof of identity:Adhaar number
            Extension extension1 = new Extension();
            Extension listextension1 = new Extension();

            listextension1.Url = "category";
            listextension1.Value = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-supportinginfo-category", "POI", "Proof of identity", "Proof of identity");
            extension1.Extension.Add(listextension1);

            listextension1.Url = "code";
            listextension1.Value = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-identifier-type-code", "ADN", "Adhaar number", "Adhaar number");
            extension1.Extension.Add(listextension1);
            extension1.Url = "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Claim-SupportingInfoRequirement";                  

            // set coverage 1 : In patient hostpitalization coverage
            InsurancePlan.CoverageComponent coverageComponent1 = new InsurancePlan.CoverageComponent();
            coverageComponent1.Type = new CodeableConcept("http://snomed.info/sct", "737481003", "Inpatient care management (procedure)", "Inpatient care management (procedure)");                 

            // set Benefit 1 : Benefit covered under Intensive care unit (environment)
            InsurancePlan.CoverageBenefitComponent coverageBenefitComponent1 = new InsurancePlan.CoverageBenefitComponent();
            coverageBenefitComponent1.Type = new CodeableConcept("http://snomed.info/sct", "309904001","Intensive care unit (environment)", "Intensive care unit (environment)");
            coverageComponent1.Benefit.Add(coverageBenefitComponent1);

            InsurancePlan.CoverageBenefitComponent coverageBenefitComponent_blood = new InsurancePlan.CoverageBenefitComponent();
            coverageBenefitComponent_blood.Type = new CodeableConcept("http://snomed.info/sct", "87612001", "Blood", "Blood");
            coverageComponent1.Benefit.Add(coverageBenefitComponent_blood);

            InsurancePlan.CoverageBenefitComponent coverageBenefitComponent_oxygen = new InsurancePlan.CoverageBenefitComponent();
            coverageBenefitComponent_oxygen.Type = new CodeableConcept("http://snomed.info/sct", "24099007", "Oxygen (substance)", "Oxygen (substance)");
            coverageComponent1.Benefit.Add(coverageBenefitComponent_oxygen);
                        

            // set coverage 2 : Post Hospitalization
            InsurancePlan.CoverageComponent coverageComponent2 = new InsurancePlan.CoverageComponent();
            coverageComponent2.Type = new CodeableConcept("http://snomed.info/sct", "710967003", "Management of health status after discharge from hospital (procedure)", "Management of health status after discharge from hospital (procedure)");

            // set benefit 2 : Benefit cover under Post Hospitalization
            InsurancePlan.CoverageBenefitComponent coverageBenefitComponent2 = new InsurancePlan.CoverageBenefitComponent();
            coverageBenefitComponent2.Type = new CodeableConcept("http://snomed.info/sct", "710967003", "Management of health status after discharge from hospital (procedure)", "Management of health status after discharge from hospital (procedure)");
            coverageComponent2.Benefit.Add(coverageBenefitComponent2);

            // set Limit 2 : available limit for the benefit2 ( Post Hospitalization )            
            InsurancePlan.LimitComponent limit2 = new InsurancePlan.LimitComponent();
            Quantity quantity2 = new Quantity();
            quantity2.Value = 60;
            quantity2.Unit = "day";
            quantity2.Comparator = Quantity.QuantityComparator.LessOrEqual;
            limit2.Value = quantity2;
            coverageBenefitComponent2.Limit.Add(limit2);
            
            // set Coverage 3 : pre Hospitalization
            InsurancePlan.CoverageComponent coverageComponent3 = new InsurancePlan.CoverageComponent();
            coverageComponent3.Type = new CodeableConcept("http://snomed.info/sct", "409972000", "Pre-hospital care (situation)", "Pre-hospital care (situation)");

            // set Benefit 3 : Benefit covered under pre Hospitalization
            InsurancePlan.CoverageBenefitComponent coverageBenefitComponent3 = new InsurancePlan.CoverageBenefitComponent();
            coverageBenefitComponent3.Type = new CodeableConcept("http://snomed.info/sct", "409972000", "Pre-hospital care (situation)", "Pre-hospital care (situation)");
            coverageComponent3.Benefit.Add(coverageBenefitComponent3);

            // set Limit 3 : available limit for the benefit3 ( pre Hospitalization )
            InsurancePlan.LimitComponent limit3 = new InsurancePlan.LimitComponent();
            Quantity quantity3 = new Quantity();
            quantity3.Value = 60;
            quantity3.Unit = "day";
            quantity3.Comparator = Quantity.QuantityComparator.LessOrEqual;
            limit3.Value = quantity3;
            coverageBenefitComponent3.Limit.Add(limit3);

            // set Coverage 4 : Ambulance coverage
            InsurancePlan.CoverageComponent coverageComponent4 = new InsurancePlan.CoverageComponent();
            coverageComponent4.Type = new CodeableConcept("http://snomed.info/sct", "49122002", "Ambulance, device (physical object)", "Ambulance, device (physical object)");

           
            // set Benefit 4 : Benefit covered under Ambulance coverage
            InsurancePlan.CoverageBenefitComponent coverageBenefitComponent4 = new InsurancePlan.CoverageBenefitComponent();
            coverageBenefitComponent4.Type = new CodeableConcept("http://snomed.info/sct", "49122002", "Ambulance, device (physical object)", "Ambulance, device(physical object)");
            coverageComponent4.Benefit.Add(coverageBenefitComponent4);


            // set Coverage 5 : Day Care Case Mangament
            InsurancePlan.CoverageComponent coverageComponent5 = new InsurancePlan.CoverageComponent();
            coverageComponent5.Type = new CodeableConcept("http://snomed.info/sct", "737850002", "Day care case management", "Day care case management");
           

            // set Benefit 5 : Benefit covered under Day care case management
            InsurancePlan.CoverageBenefitComponent coverageBenefitComponent5 = new InsurancePlan.CoverageBenefitComponent();
            coverageBenefitComponent5.Type = new CodeableConcept("http://snomed.info/sct", "737850002", "Day care case management", "Day care case management");
            coverageComponent5.Benefit.Add(coverageBenefitComponent5);

            // set Coverage 6 : Organ Donar 
            InsurancePlan.CoverageComponent coverageComponent6 = new InsurancePlan.CoverageComponent();
            coverageComponent6.Type = new CodeableConcept("http://snomed.info/sct", "105461009", "Organ donor", "Organ donor");

          

            // set Benefit 6 : Benefit covered under Day care case management
            InsurancePlan.CoverageBenefitComponent coverageBenefitComponent6 = new InsurancePlan.CoverageBenefitComponent();
            coverageBenefitComponent6.Type = new CodeableConcept("http://snomed.info/sct", "105461009", "Organ donor", "Organ donor");
            coverageComponent6.Benefit.Add(coverageBenefitComponent6);   
                           
            insurancePlan.Coverage.Add(coverageComponent1);
            insurancePlan.Coverage.Add(coverageComponent2);
            insurancePlan.Coverage.Add(coverageComponent3);
            insurancePlan.Coverage.Add(coverageComponent4);
            insurancePlan.Coverage.Add(coverageComponent5);
            insurancePlan.Coverage.Add(coverageComponent6);           


            // Plan 1 : Silver ( individual )
            Identifier identifier1 = new Identifier();
            InsurancePlan.PlanComponent planComponent2 = new InsurancePlan.PlanComponent();
            identifier1 = new Identifier();
            identifier1.Value = "Activ assure Silver";
            identifier1.Use = Identifier.IdentifierUse.Official;
            planComponent2.Identifier.Add(identifier1);

            planComponent2.Type = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-plan-type", "01", "Individual", "Individual");

            // Sum insured under : Silver ( Individual ) Genral Cost
            InsurancePlan.GeneralCostComponent sumInsuredSilver1 = new InsurancePlan.GeneralCostComponent();
            Money money4 = new Money();
            money4.Currency = Money.Currencies.INR;
            money4.Value = 200000;
            sumInsuredSilver1.Cost = money4;
            planComponent2.GeneralCost.Add(sumInsuredSilver1);      

            // Specific Cost 
            InsurancePlan.SpecificCostComponent sumInsuredSilverSpecificCost = new InsurancePlan.SpecificCostComponent();
            sumInsuredSilverSpecificCost.Category = new CodeableConcept("http://snomed.info/sct", "49122002", "Ambulance, device (physical object)", "Ambulance, device (physical object)");

            InsurancePlan.PlanBenefitComponent planbenfitcomponat1 = new InsurancePlan.PlanBenefitComponent();
            planbenfitcomponat1.Type= new CodeableConcept("http://snomed.info/sct", "49122002", "Ambulance, device (physical object)", "Ambulance, device (physical object)");

            InsurancePlan.CostComponent plancostcomponant1 = new InsurancePlan.CostComponent();
            plancostcomponant1.Type = new CodeableConcept("","","","full coverage");
            Quantity q1 = new Quantity();
            q1.Value = 2000;
            q1.Unit = "INR";
            plancostcomponant1.Value = q1;
            planbenfitcomponat1.Cost.Add(plancostcomponant1);
            sumInsuredSilverSpecificCost.Benefit.Add(planbenfitcomponat1);

            planComponent2.SpecificCost.Add(sumInsuredSilverSpecificCost);


            // Plan 3 : Silver ( individual )
            Identifier identifier3_1 = new Identifier();
            InsurancePlan.PlanComponent planComponent3_2 = new InsurancePlan.PlanComponent();
            identifier3_1 = new Identifier();
            identifier3_1.Value = "Activ assure Silver";
            identifier3_1.Use = Identifier.IdentifierUse.Official;
            planComponent3_2.Identifier.Add(identifier1);

            planComponent3_2.Type = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-plan-type", "01", "Individual", "Individual");

            // Sum insured under : Silver ( Individual ) Genral Cost
            InsurancePlan.GeneralCostComponent sumInsuredSilver3_1 = new InsurancePlan.GeneralCostComponent();
            Money money3_4 = new Money();
            money3_4.Currency = Money.Currencies.INR;
            money3_4.Value = 700000;
            sumInsuredSilver3_1.Cost = money3_4;
            planComponent3_2.GeneralCost.Add(sumInsuredSilver3_1);

            // Specific Cost 1
            InsurancePlan.SpecificCostComponent sumInsuredSilverSpecificCost3 = new InsurancePlan.SpecificCostComponent();
            sumInsuredSilverSpecificCost3.Category = new CodeableConcept("http://snomed.info/sct", "49122002", "Ambulance, device (physical object)", "Ambulance, device (physical object)");

            InsurancePlan.PlanBenefitComponent planbenfitcomponat3_1 = new InsurancePlan.PlanBenefitComponent();
            planbenfitcomponat3_1.Type = new CodeableConcept("http://snomed.info/sct", "49122002", "Ambulance, device (physical object)", "Ambulance, device (physical object)");

            InsurancePlan.CostComponent plancostcomponant3_1 = new InsurancePlan.CostComponent();
            plancostcomponant3_1.Type = new CodeableConcept("", "", "", "full coverage");
            Quantity q3_1 = new Quantity();
            q3_1.Value = 2000;
            q3_1.Unit = "INR";
            plancostcomponant3_1.Value = q3_1;
            planbenfitcomponat3_1.Cost.Add(plancostcomponant3_1);
            sumInsuredSilverSpecificCost3.Benefit.Add(planbenfitcomponat3_1);

            planComponent3_2.SpecificCost.Add(sumInsuredSilverSpecificCost3);

            // Specific Cost 2
            InsurancePlan.SpecificCostComponent sumInsuredSilverSpecificCost2_3 = new InsurancePlan.SpecificCostComponent();
            sumInsuredSilverSpecificCost2_3.Category = new CodeableConcept("http://snomed.info/sct", "224663004", "Single room (environment)", "Single room (environment)");

            InsurancePlan.PlanBenefitComponent planbenfitcomponat2_3_1 = new InsurancePlan.PlanBenefitComponent();
            planbenfitcomponat2_3_1.Type = new CodeableConcept("http://snomed.info/sct", "224663004", "Single room (environment)", "Single room (environment)");

            InsurancePlan.CostComponent plancostcomponant2_3_1 = new InsurancePlan.CostComponent();
            plancostcomponant2_3_1.Type = new CodeableConcept("", "", "", "full coverage");
            Quantity q2_3_1 = new Quantity();
            q2_3_1.Value = 7000;
            q2_3_1.Unit = "INR";
            plancostcomponant2_3_1.Value = q2_3_1;
            planbenfitcomponat2_3_1.Cost.Add(plancostcomponant2_3_1);
            sumInsuredSilverSpecificCost2_3.Benefit.Add(planbenfitcomponat2_3_1);

            planComponent3_2.SpecificCost.Add(sumInsuredSilverSpecificCost2_3);

            // Specific Cost 3
            sumInsuredSilverSpecificCost3 = new InsurancePlan.SpecificCostComponent();
            sumInsuredSilverSpecificCost3.Category = new CodeableConcept("http://snomed.info/sct", "309904001", "Intensive care unit (environment)", "Intensive care unit (environment)");

            InsurancePlan.PlanBenefitComponent planbenfitcomponat3 = new InsurancePlan.PlanBenefitComponent();
            planbenfitcomponat3.Type = new CodeableConcept("http://snomed.info/sct", "309904001", "Intensive care unit (environment)", "Intensive care unit (environment)");

            InsurancePlan.CostComponent plancostcomponant3 = new InsurancePlan.CostComponent();
            plancostcomponant3.Type = new CodeableConcept("", "", "", "full coverage");
            Quantity q3 = new Quantity();
            q3.Value = 14000;
            q3.Unit = "INR";
            plancostcomponant3.Value = q3;
            planbenfitcomponat3.Cost.Add(plancostcomponant3);
            sumInsuredSilverSpecificCost3.Benefit.Add(planbenfitcomponat3);

            planComponent3_2.SpecificCost.Add(sumInsuredSilverSpecificCost3);

            // Specific Cost 4
            InsurancePlan.SpecificCostComponent sumInsuredSilverSpecificCost4 = new InsurancePlan.SpecificCostComponent();
            sumInsuredSilverSpecificCost4.Category = new CodeableConcept("http://snomed.info/sct", "60689008", "Home care of patient", "Home care of patient");

            InsurancePlan.PlanBenefitComponent planbenfitcomponat4 = new InsurancePlan.PlanBenefitComponent();
            planbenfitcomponat4.Type = new CodeableConcept("http://snomed.info/sct", "60689008", "Home care of patient", "Home care of patient");

            InsurancePlan.CostComponent plancostcomponant4 = new InsurancePlan.CostComponent();
            plancostcomponant4.Type = new CodeableConcept("", "", "", "full coverage");
            Quantity q4 = new Quantity();
            q4.Value = 70000;
            q4.Unit = "INR";
            plancostcomponant4.Value = q4;
            planbenfitcomponat4.Cost.Add(plancostcomponant4);
            sumInsuredSilverSpecificCost4.Benefit.Add(planbenfitcomponat4);

            planComponent3_2.SpecificCost.Add(sumInsuredSilverSpecificCost4);
                        
            insurancePlan.Plan.Add(planComponent2);
            insurancePlan.Plan.Add(planComponent3_2);
           
            return insurancePlan;
        }

        //populate the claim resource
        public static Claim populateClaimResource(Use claimUse)
        {
            // set Meta - Metadata about the resource
            Claim claim = new Claim()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Claim",
                    },
                },
            };

            // set Id - Logical id of this artifact
            claim.Id = "Claim-01";

            // set Identifier - Business Identifier for claim
            var id = new Identifier();
            id.System = "https://irdai.gov.in";
            id.Value = "7612345";
            claim.Identifier.Add(id);

            // set Status - active | cancelled | draft | entered-in-error
            claim.Status = FinancialResourceStatusCodes.Active;

            // set Type - Category or discipline
            claim.Type = new CodeableConcept("http://snomed.info/sct", "710967003", "Management of health status after discharge from hospital (procedure)", "Management of health status after discharge from hospital (procedure)");

            //set Sub-Type
            claim.SubType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/ex-claimsubtype", "emergency", "Emergency Claim");

            // set Use - claim | preauthorization | predetermination
            switch (claimUse)
            {
                case Use.Claim:
                    claim.Use = claimUse;
                    break;

                case Use.Preauthorization:
                    claim.Use = claimUse;
                    break;

                case Use.Predetermination:
                    claim.Use = claimUse;
                    break;
                default:
                    break;
            }

            // set Patient - The recipient of the products and services
            claim.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");

            // set Created - Resource creation date
            claim.CreatedElement = new FhirDateTime("2020-07-10");

            // set Insurer - Target
            claim.Insurer = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set Provider - Party responsible for the claim
            claim.Provider = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set prority - Desired processing ugency
            claim.Priority = new CodeableConcept("http://terminology.hl7.org/CodeSystem/processpriority", "normal", "Normal");

            // set Prescription provided by practitioner
            claim.Prescription = new ResourceReference("urn:uuid:b8970fa7-ae4a-47b4-9405-e382b3f7c055");

            //set careTeam          
            Claim.CareTeamComponent CareTeamComponent = new Claim.CareTeamComponent();
            CareTeamComponent.Role = new CodeableConcept("http://snomed.info/sct", "768839008", "Consultant", "Consultant");
            CareTeamComponent.Qualification = new CodeableConcept("http://snomed.info/sct", "394658006", "Clinical specialty (qualifier value)", "Clinical specialty (qualifier value)");
            CareTeamComponent.SequenceElement = new PositiveInt(1);
            CareTeamComponent.Provider = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e");
            claim.CareTeam.Add(CareTeamComponent);

            // set supportingInfo - Supporting information
            Claim.SupportingInformationComponent SupportingInformationComponent = new Claim.SupportingInformationComponent();

            SupportingInformationComponent.Sequence = 1;
            SupportingInformationComponent.Category = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-supportinginfo-category", "POI", "proof of identity", "proof of identity");
            SupportingInformationComponent.Code = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-identifier-type-code", "ADN", "Adhaar number", "Adhaar number");

            claim.SupportingInfo.Add(SupportingInformationComponent);           

            // set Procedure - Clinical procedures performed
            Claim.ProcedureComponent ProcedureComponent = new Claim.ProcedureComponent();
            ProcedureComponent.Sequence = 1;
            ProcedureComponent.Procedure = new CodeableConcept("http://snomed.info/sct", "768839008", "Consultant", "Consultant");
            claim.Procedure.Add(ProcedureComponent);

            // set diagnosis - Pertinent diagnosis information
            Claim.DiagnosisComponent DiagnosisComponent = new Claim.DiagnosisComponent();
            DiagnosisComponent.Sequence = 1;
            DiagnosisComponent.PackageCode = new CodeableConcept("http://hl7.org/fhir/sid/icd-10", "E11.9", "Type 2 diabetes mellitus : Without complications", "Type 2 diabetes mellitus : Without complications");
            DiagnosisComponent.Type.Add(new CodeableConcept("http://snomed.info/sct", "89100005", "Final diagnosis (discharge) (contextual qualifier) (qualifier value)", "Final diagnosis (discharge) (contextual qualifier) (qualifier value)"));
            DiagnosisComponent.Diagnosis = new CodeableConcept("http://hl7.org/fhir/sid/icd-10", "B53.0", "Plasmodium ovale malaria", "Plasmodium ovale malaria");
            claim.Diagnosis.Add(DiagnosisComponent);

            // set Insurance - Patient insurance information
            Claim.InsuranceComponent InsuranceComponent = new Claim.InsuranceComponent();
            InsuranceComponent.Sequence = 1;
            InsuranceComponent.Focal = true;
            InsuranceComponent.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");
            IEnumerable<string> m_oEnum = new string[] { "12S52" };
            InsuranceComponent.PreAuthRef = m_oEnum;
            claim.Insurance.Add(InsuranceComponent);

            // set Item - Product or service provided
            Claim.ItemComponent item1 = new Claim.ItemComponent();
            item1.Sequence = 1;
            item1.CareTeamSequenceElement.Add(new PositiveInt(1));
            item1.ProductOrService = new CodeableConcept("http://snomed.info/sct", "768839008", "Consultant", "Consultant");
            Money money = new Money();
            money.Currency = Money.Currencies.INR;
            money.Value = (decimal)500.00;
            item1.UnitPrice = money;
            item1.Net = money;

            Claim.ItemComponent item2 = new Claim.ItemComponent();
            item2.Sequence = 2;
            item2.CareTeamSequenceElement.Add(new PositiveInt(1));
            item2.ProductOrService = new CodeableConcept("http://snomed.info/sct", "768839008", "Metformin hydrochloride 500 mg prolonged-release oral tablet", "Metformin hydrochloride 500 mg prolonged-release oral tablet");
            Money money1 = new Money();
            money1.Currency = Money.Currencies.INR;
            money1.Value = (decimal)910.28;
            item2.UnitPrice = money1;
            item2.Net = money1;

            claim.Item.Add(item1);
            claim.Item.Add(item2);

            Claim.DiagnosisComponent diagnosiscomponent = new Claim.DiagnosisComponent();
            diagnosiscomponent.Sequence = 1;
            diagnosiscomponent.Diagnosis = new CodeableConcept("http://hl7.org/fhir/sid/icd-10", "B53.0", "Plasmodium ovale malaria", "Plasmodium ovale malaria");
            diagnosiscomponent.Type.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/ex-diagnosistype", "admitting", "Admitting Diagnosis", "Admitting Diagnosis"));

            claim.Diagnosis.Add(diagnosiscomponent);

            return claim;

        }

        //populate the claim resource - Claim-enhancement-01  
        //Claim/Claim-enhancement-01
        public static Claim populateClaimenhancementResource()
        {
            // set Meta - Metadata about the resource
            Claim claim = new Claim()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Claim",
                    },
                },
            };

            // set Id - Logical id of this artifact
            claim.Id = "7aace234-5172-4126-a907-ace8745bd1a5";            

            // set Identifier - Business Identifier for claim
            var id = new Identifier();
            id.System = "https://irdai.gov.in";
            id.Value = "7612345";
            claim.Identifier.Add(id);

            // set Status - active | cancelled | draft | entered-in-error
            claim.Status = FinancialResourceStatusCodes.Active;

            // set Type - Category or discipline
            claim.Type = new CodeableConcept("http://snomed.info/sct", "737481003", "Inpatient care management (procedure)", "Inpatient care management");

            //set Sub-Type
            claim.SubType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/ex-claimsubtype", "emergency", "Emergency Claim");

            claim.Use = Use.Preauthorization;

            // set Patient - The recipient of the products and services
            claim.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");       //"Patient/example-01"

            // set BillablePeriod   
            claim.BillablePeriod = new Period();
                    
            claim.BillablePeriod.Start = "2023-12-09T11:01:00+05:00";
            claim.BillablePeriod.End = "2023-12-30T11:01:00+05:00";

            // set Created - Resource creation date
            claim.Created = "2023-12-13T11:01:00+05:00";

            // set Insurer - Target
            claim.Insurer = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");        // Insurance Company 


            // set Provider - Party responsible for the claim
            claim.Provider = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");           // Hospital 


            // set prority - Desired processing ugency
            claim.Priority = new CodeableConcept("http://terminology.hl7.org/CodeSystem/processpriority", "normal", "Normal");

            // set Prescription provided by practitioner
            claim.Prescription = new ResourceReference("urn:uuid:acefdfbd-e612-483e-90fc-a5c44d09a4b9");  

            // set related Claim for enhancement 
            Claim.RelatedClaimComponent relatedclaimcomponant = new Claim.RelatedClaimComponent();

            relatedclaimcomponant.Claim = new ResourceReference("urn:uuid:760ec93e-7ec8-4e82-b8a3-fe6512fccd8b");          
            relatedclaimcomponant.Relationship = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-related-claim-relationship-code", "enhancement", "Enhancement", "Enhancement");
            claim.Related.Add(relatedclaimcomponant);

            //set careTeam          
            Claim.CareTeamComponent CareTeamComponent = new Claim.CareTeamComponent();
            CareTeamComponent.SequenceElement = new PositiveInt(1);
            CareTeamComponent.Provider = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e");  //"Practitioner/example-01"
            CareTeamComponent.Role = new CodeableConcept("http://snomed.info/sct", "223366009", "Healthcare professional (occupation)", "Healthcare professional (occupation)");
            CareTeamComponent.Qualification = new CodeableConcept("http://snomed.info/sct", "394658006", "Clinical specialty (qualifier value)", "Clinical specialty (qualifier value)");

            claim.CareTeam.Add(CareTeamComponent);

            // set supportingInfo - Supporting information
            Claim.SupportingInformationComponent SupportingInformationComponent = new Claim.SupportingInformationComponent();

            SupportingInformationComponent.Sequence = 1;
            SupportingInformationComponent.Category = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-supportinginfo-category", "INV", "Document Type - Investigation", "Document Type - Investigation");
            SupportingInformationComponent.Code = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");
                        
            var documentReference = new ResourceReference("urn:uuid:e53fa5db-f676-4b16-a273-f4088866314e");
            SupportingInformationComponent.Value = documentReference;

            claim.SupportingInfo.Add(SupportingInformationComponent);

            // set diagnosis - Pertinent diagnosis information
            Claim.DiagnosisComponent DiagnosisComponent = new Claim.DiagnosisComponent();
            DiagnosisComponent.Sequence = 1;
            DiagnosisComponent.Diagnosis = new CodeableConcept("http://hl7.org/fhir/sid/icd-10", "I46.9", "Cardiac arrest, unspecified", "Cardiac arrest, unspecified");
            DiagnosisComponent.Type.Add(new CodeableConcept("http://snomed.info/sct", "89100005", "Final diagnosis (discharge) (contextual qualifier) (qualifier value)", "Final diagnosis (discharge) (contextual qualifier) (qualifier value)"));
           
            claim.Diagnosis.Add(DiagnosisComponent);

            // set Procedure - Clinical procedures performed
            Claim.ProcedureComponent ProcedureComponent = new Claim.ProcedureComponent();
            ProcedureComponent.Sequence = 1;
            ProcedureComponent.Procedure = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");
            claim.Procedure.Add(ProcedureComponent);
            ProcedureComponent.Sequence = 2;
            ProcedureComponent.Procedure = new CodeableConcept("http://snomed.info/sct", "418285008", "Angioplasty of blood vessel", "Angioplasty of blood vessel");
            claim.Procedure.Add(ProcedureComponent);

            // set Insurance - Patient insurance information
            Claim.InsuranceComponent InsuranceComponent = new Claim.InsuranceComponent();
            InsuranceComponent.Sequence = 1;
            InsuranceComponent.Focal = true;
            InsuranceComponent.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");        // 	  "Coverage/example-02"
            IEnumerable<string> m_oEnum = new string[] { "123456" };
            InsuranceComponent.PreAuthRef = m_oEnum;
            claim.Insurance.Add(InsuranceComponent);

            // set Item - Product or service provided
            Claim.ItemComponent item1 = new Claim.ItemComponent();
            item1.Sequence = 1;
            item1.CareTeamSequenceElement.Add(new PositiveInt(1));
            item1.ProductOrService = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");           

            Money money = new Money();
            money.Currency = Money.Currencies.INR;
            money.Value = (decimal)10000.00;
            item1.UnitPrice = money;
            item1.Net = money;

            Claim.ItemComponent item2 = new Claim.ItemComponent();
            item2.Sequence = 2;
            item2.CareTeamSequenceElement.Add(new PositiveInt(1));
            item2.ProductOrService = new CodeableConcept("http://snomed.info/sct", "418285008", "Angioplasty of blood vessel", "Angioplasty of blood vessel");
            Money money1 = new Money();
            money1.Currency = Money.Currencies.INR;
            money1.Value = (decimal)10000;
            item2.UnitPrice = money1;
            item2.Net = money1;
                         
            claim.Item.Add(item1);
            claim.Item.Add(item2);
             
            Money money5 = new Money();
            money5.Currency = Money.Currencies.INR;
            money5.Value = (decimal)20000;

            claim.Total = money5;

            return claim;
        }

        //populate the claim resource - Claim-preauth-example-01
        public static Claim populateClaimpreauthResource()
        {
            // set Meta - Metadata about the resource
            Claim claim = new Claim()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Claim",
                    },
                },
            };

            // set Id - Logical id of this artifact
            claim.Id = "760ec93e-7ec8-4e82-b8a3-fe6512fccd8b";            


            // set Identifier - Business Identifier for claim
            var id = new Identifier();
            id.System = "https://irdai.gov.in";
            id.Value = "7612345";
            claim.Identifier.Add(id);

            // set Status - active | cancelled | draft | entered-in-error
            claim.Status = FinancialResourceStatusCodes.Active;

            // set Type - Category or discipline
            claim.Type = new CodeableConcept("http://snomed.info/sct", "737481003", "Inpatient care management (procedure)");

            //set Sub-Type
            claim.SubType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/ex-claimsubtype", "emergency", "Emergency Claim");

            claim.Use = Use.Preauthorization;

            // set Patient - The recipient of the products and services
            claim.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");       //"Patient/example-01"

            // set BillablePeriod  
            claim.BillablePeriod = new Period();
            claim.BillablePeriod.Start = "2023-12-09T11:01:00+05:00";
            claim.BillablePeriod.End = "2023-12-30T11:01:00+05:00";

            // set Created - Resource creation date
            claim.Created = "2023-12-11T11:01:00+05:00";

            // set Insurer - Target
            claim.Insurer = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");    // "Organization/example-02"   // Insurance Company 


            // set Provider - Party responsible for the claim
            claim.Provider = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f"); //"Organization/example-03"      // Hospital 


            // set prority - Desired processing ugency
            claim.Priority = new CodeableConcept("http://terminology.hl7.org/CodeSystem/processpriority", "normal", "Normal");

            // set Prescription provided by practitioner    
            claim.Prescription = new ResourceReference("urn:uuid:acefdfbd-e612-483e-90fc-a5c44d09a4b9");   

            //set careTeam          
            Claim.CareTeamComponent CareTeamComponent = new Claim.CareTeamComponent();
            CareTeamComponent.SequenceElement = new PositiveInt(1);
            CareTeamComponent.Provider = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e");  //"Practitioner/example-01"
            CareTeamComponent.Role = new CodeableConcept("http://snomed.info/sct", "223366009", "Healthcare professional (occupation)", "Healthcare professional (occupation)");
            CareTeamComponent.Qualification = new CodeableConcept("http://snomed.info/sct", "394658006", "Clinical specialty (qualifier value)", "Clinical specialty (qualifier value)");

            claim.CareTeam.Add(CareTeamComponent);

            // set supportingInfo - Supporting information
            Claim.SupportingInformationComponent SupportingInformationComponent = new Claim.SupportingInformationComponent();

            SupportingInformationComponent.Sequence = 1;
            SupportingInformationComponent.Category = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-supportinginfo-category", "INV", "Document Type - Investigation", "Document Type - Investigation");
            SupportingInformationComponent.Code = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");

            var documentReference = new ResourceReference("urn:uuid:e53fa5db-f676-4b16-a273-f4088866314e");
            SupportingInformationComponent.Value = documentReference;

            claim.SupportingInfo.Add(SupportingInformationComponent);

            // set diagnosis - Pertinent diagnosis information
            Claim.DiagnosisComponent DiagnosisComponent = new Claim.DiagnosisComponent();
            DiagnosisComponent.Sequence = 1;
            DiagnosisComponent.Diagnosis = new CodeableConcept("http://hl7.org/fhir/sid/icd-10", "I46.9", "Cardiac arrest, unspecified", "Cardiac arrest, unspecified");
            DiagnosisComponent.Type.Add(new CodeableConcept("http://snomed.info/sct", "89100005", "Final diagnosis (discharge) (contextual qualifier) (qualifier value)", "Final diagnosis (discharge) (contextual qualifier) (qualifier value)"));

            claim.Diagnosis.Add(DiagnosisComponent);

            // set Procedure - Clinical procedures performed
            Claim.ProcedureComponent ProcedureComponent = new Claim.ProcedureComponent();
            ProcedureComponent.Sequence = 1;
            ProcedureComponent.Procedure = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");
            claim.Procedure.Add(ProcedureComponent);
            ProcedureComponent.Sequence = 2;
            ProcedureComponent.Procedure = new CodeableConcept("http://snomed.info/sct", "418285008", "Angioplasty of blood vessel", "Angioplasty of blood vessel");
            claim.Procedure.Add(ProcedureComponent);

            // set Insurance - Patient insurance information
            Claim.InsuranceComponent InsuranceComponent = new Claim.InsuranceComponent();
            InsuranceComponent.Sequence = 1;
            InsuranceComponent.Focal = true;
            InsuranceComponent.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");        // 	  "Coverage/example-02"
            IEnumerable<string> m_oEnum = new string[] { "123456" };
            InsuranceComponent.PreAuthRef = m_oEnum;
            claim.Insurance.Add(InsuranceComponent);

            // set Item - Product or service provided
            Claim.ItemComponent item1 = new Claim.ItemComponent();
            item1.Sequence = 1;
            item1.CareTeamSequenceElement.Add(new PositiveInt(1));
            item1.ProductOrService = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");

            Money money = new Money();
            money.Currency = Money.Currencies.INR;
            money.Value = (decimal)1000.00;
            item1.UnitPrice = money;
            item1.Net = money;

            Claim.ItemComponent item2 = new Claim.ItemComponent();
            item2.Sequence = 2;
            item2.CareTeamSequenceElement.Add(new PositiveInt(1));
            item2.ProductOrService = new CodeableConcept("http://snomed.info/sct", "418285008", "Angioplasty of blood vessel", "Angioplasty of blood vessel");
            Money money1 = new Money();
            money1.Currency = Money.Currencies.INR;
            money1.Value = (decimal)40000;
            item2.UnitPrice = money1;
            item2.Net = money1;

            Claim.ItemComponent item3 = new Claim.ItemComponent();
            item3.Sequence = 3;
            item3.CareTeamSequenceElement.Add(new PositiveInt(1));
            item3.ProductOrService = new CodeableConcept("http://snomed.info/sct", "309904001", "Intensive care unit", "Intensive care unit");
            Quantity q3 = new Quantity();
            q3.Value = 10;
            q3.Unit = "day";
            item3.Quantity = q3;
            Money money3 = new Money();
            money3.Currency = Money.Currencies.INR;
            money3.Value = (decimal)2000;
            item3.UnitPrice = money3;

            Claim.ItemComponent item4 = new Claim.ItemComponent();
            item4.Sequence = 4;
            item4.CareTeamSequenceElement.Add(new PositiveInt(1));
            item4.ProductOrService = new CodeableConcept("http://snomed.info/sct", "319775004", "Aspirin 75 mg oral tablet", "Aspirin 75 mg oral tablet");

            Money money4 = new Money();
            money4.Currency = Money.Currencies.INR;
            money4.Value = (decimal)100;
            item4.UnitPrice = money4;
            item4.Net = money4;
            claim.Item.Add(item1);
            claim.Item.Add(item2);
            claim.Item.Add(item3);
            claim.Item.Add(item4);

            Money money5 = new Money();
            money5.Currency = Money.Currencies.INR;
            money5.Value = (decimal)100;

            claim.Total = money5;

            return claim;
        }

        //populate the claim resource - Claim-predetermination-01
        public static Claim populateClaimpredeterminationResource()
        {
            // set Meta - Metadata about the resource
            Claim claim = new Claim()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Claim",
                    },
                },
            };

            // set Id - Logical id of this artifact
            claim.Id ="372a5471-1e67-4501-8c29-b20b783ba33e"; 

            // set Identifier - Business Identifier for claim
            var id = new Identifier();
            id.System = "https://irdai.gov.in";
            id.Value = "7612345";
            claim.Identifier.Add(id);

            // set Status - active | cancelled | draft | entered-in-error
            claim.Status = FinancialResourceStatusCodes.Active;

            // set Type - Category or discipline
            claim.Type = new CodeableConcept("http://snomed.info/sct", "737481003", "Inpatient care management (procedure)", "Inpatient care management");

            //set Sub-Type
            claim.SubType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/ex-claimsubtype", "emergency", "Emergency Claim");

            claim.Use = Use.Predetermination;

            // set Patient - The recipient of the products and services
            claim.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");       

            // set BillablePeriod    
            claim.BillablePeriod = new Period();
            claim.BillablePeriod.Start = "2023-12-09T11:01:00+05:00";
            claim.BillablePeriod.End = "2023-12-30T11:01:00+05:00";

            // set Created - Resource creation date
            claim.Created = "2023-12-11T11:01:00+05:00";

            // set Insurer - Target
            claim.Insurer = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");    // "Organization/example-02"   // Insurance Company 


            // set Provider - Party responsible for the claim
            claim.Provider = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f"); //"Organization/example-03"      // Hospital 


            // set prority - Desired processing ugency
            claim.Priority = new CodeableConcept("http://terminology.hl7.org/CodeSystem/processpriority", "normal", "Normal");

            // set Prescription provided by practitioner
            claim.Prescription = new ResourceReference("urn:uuid:acefdfbd-e612-483e-90fc-a5c44d09a4b9");   //"MedicationRequest/example-03"

            //set careTeam          
            Claim.CareTeamComponent CareTeamComponent = new Claim.CareTeamComponent();
            CareTeamComponent.SequenceElement = new PositiveInt(1);
            CareTeamComponent.Provider = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e");  //"Practitioner/example-01"
            CareTeamComponent.Role = new CodeableConcept("http://snomed.info/sct", "223366009", "Healthcare professional (occupation)", "Healthcare professional (occupation)");
            CareTeamComponent.Qualification = new CodeableConcept("http://snomed.info/sct", "394658006", "Clinical specialty (qualifier value)", "Clinical specialty (qualifier value)");

            claim.CareTeam.Add(CareTeamComponent);

            // set supportingInfo - Supporting information
            Claim.SupportingInformationComponent SupportingInformationComponent = new Claim.SupportingInformationComponent();

            SupportingInformationComponent.Sequence = 1;
            SupportingInformationComponent.Category = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-supportinginfo-category", "INV", "Document Type - Investigation", "Document Type - Investigation");
            SupportingInformationComponent.Code = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");
            var documentReference = new ResourceReference("urn:uuid:e53fa5db-f676-4b16-a273-f4088866314e");
            SupportingInformationComponent.Value = documentReference;
            claim.SupportingInfo.Add(SupportingInformationComponent);

            // set diagnosis - Pertinent diagnosis information
            Claim.DiagnosisComponent DiagnosisComponent = new Claim.DiagnosisComponent();
            DiagnosisComponent.Sequence = 1;
            DiagnosisComponent.Diagnosis = new CodeableConcept("http://hl7.org/fhir/sid/icd-10", "I46.9", "Cardiac arrest, unspecified", "Cardiac arrest, unspecified");
            DiagnosisComponent.Type.Add(new CodeableConcept("http://snomed.info/sct", "89100005", "Final diagnosis (discharge) (contextual qualifier) (qualifier value)", "Final diagnosis (discharge) (contextual qualifier) (qualifier value)"));
            
            claim.Diagnosis.Add(DiagnosisComponent);

            // set Procedure - Clinical procedures performed
            Claim.ProcedureComponent ProcedureComponent = new Claim.ProcedureComponent();
            ProcedureComponent.Sequence = 1;
            ProcedureComponent.Procedure = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");
            claim.Procedure.Add(ProcedureComponent);
            ProcedureComponent.Sequence = 2;
            ProcedureComponent.Procedure = new CodeableConcept("http://snomed.info/sct", "418285008", "Angioplasty of blood vessel", "Angioplasty of blood vessel");
            claim.Procedure.Add(ProcedureComponent);

            // set Insurance - Patient insurance information
            Claim.InsuranceComponent InsuranceComponent = new Claim.InsuranceComponent();
            InsuranceComponent.Sequence = 1;
            InsuranceComponent.Focal = true;
            InsuranceComponent.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");        // 	  "Coverage/example-02"
            IEnumerable<string> m_oEnum = new string[] { "123456" };
            InsuranceComponent.PreAuthRef = m_oEnum;
            claim.Insurance.Add(InsuranceComponent);

            // set Item - Product or service provided
            Claim.ItemComponent item1 = new Claim.ItemComponent();
            item1.Sequence = 1;
            item1.CareTeamSequenceElement.Add(new PositiveInt(1));
            item1.ProductOrService = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");

            Money money = new Money();
            money.Currency = Money.Currencies.INR;
            money.Value = (decimal)10000.00;
            item1.UnitPrice = money;
            item1.Net = money;

            Claim.ItemComponent item2 = new Claim.ItemComponent();
            item2.Sequence = 2;
            item2.CareTeamSequenceElement.Add(new PositiveInt(1));
            item2.ProductOrService = new CodeableConcept("http://snomed.info/sct", "418285008", "Angioplasty of blood vessel", "Angioplasty of blood vessel");
            Money money1 = new Money();
            money1.Currency = Money.Currencies.INR;
            money1.Value = (decimal)10000;
            item2.UnitPrice = money1;
            item2.Net = money1;

            Claim.ItemComponent item3 = new Claim.ItemComponent();
            item3.Sequence = 3;
            item3.CareTeamSequenceElement.Add(new PositiveInt(1));
            item3.ProductOrService = new CodeableConcept("http://snomed.info/sct", "309904001", "Intensive care unit", "Intensive care unit");
            Quantity q3 = new Quantity();
            q3.Value = 10;
            q3.Unit = "day";
            item3.Quantity = q3;
            Money money3 = new Money();
            money3.Currency = Money.Currencies.INR;
            money3.Value = (decimal)2000;
            item3.UnitPrice = money3;

            Claim.ItemComponent item4 = new Claim.ItemComponent();
            item4.Sequence = 4;
            item4.CareTeamSequenceElement.Add(new PositiveInt(1));
            item4.ProductOrService = new CodeableConcept("http://snomed.info/sct", "319775004", "Aspirin 75 mg oral tablet", "Aspirin 75 mg oral tablet");

            Money money4 = new Money();
            money4.Currency = Money.Currencies.INR;
            money4.Value = (decimal)100;
            item4.UnitPrice = money4;
            item4.Net = money4;

            claim.Item.Add(item1);
            claim.Item.Add(item2);
            claim.Item.Add(item3);
            claim.Item.Add(item4);

            Money money5 = new Money();
            money5.Currency = Money.Currencies.INR;
            money5.Value = (decimal)70100;

            claim.Total = money5;

            return claim;
        }

        //populate the claim resource - populateClaimsettlement
        public static Claim populateClaimsettlement()
        {
            // set Meta - Metadata about the resource
            Claim claim = new Claim()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Claim",
                    },
                },
            };

            // set Id - Logical id of this artifact
            claim.Id = "4776dbdf-d596-4cd1-9966-9d44ae9dec0b";            

            // set Identifier - Business Identifier for claim
            var id = new Identifier();
            id.System = "https://irdai.gov.in";
            id.Value = "7612345";
            claim.Identifier.Add(id);

            // set Status - active | cancelled | draft | entered-in-error
            claim.Status = FinancialResourceStatusCodes.Active;

            // set Type - Category or discipline
            claim.Type = new CodeableConcept("http://snomed.info/sct", "737481003", "Inpatient care management (procedure)", "Inpatient care management");

            //set Sub-Type
            claim.SubType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/ex-claimsubtype", "emergency", "Emergency Claim");

            claim.Use = Use.Claim;

            // set Patient - The recipient of the products and services
            claim.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");       //"Patient/example-01"

            // set BillablePeriod    
            claim.BillablePeriod = new Period();
            claim.BillablePeriod.Start = "2023-12-09T11:01:00+05:00";
            claim.BillablePeriod.End = "2023-12-30T11:01:00+05:00";

            // set Created - Resource creation date
            claim.Created = "2023-12-11T11:01:00+05:00";

            // set Insurer - Target
            claim.Insurer = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");    // "Organization/example-02"   // Insurance Company 

    
            // set Provider - Party responsible for the claim
            claim.Provider = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f"); //"Organization/example-03"      // Hospital 


            // set prority - Desired processing ugency
            claim.Priority = new CodeableConcept("http://terminology.hl7.org/CodeSystem/processpriority", "normal", "Normal");

            // set releated - Desired processing ugency
            Claim.RelatedClaimComponent relatedClaimComponent = new Claim.RelatedClaimComponent();
            relatedClaimComponent.Claim = new ResourceReference("urn:uuid:7aace234-5172-4126-a907-ace8745bd1a5");
            relatedClaimComponent.Relationship = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-related-claim-relationship-code", "settlement", "Settlement", "Settlement");
            claim.Related.Add(relatedClaimComponent);

            // set Prescription provided by practitioner
            claim.Prescription = new ResourceReference("urn:uuid:acefdfbd-e612-483e-90fc-a5c44d09a4b9");   

            //set careTeam          
            Claim.CareTeamComponent CareTeamComponent = new Claim.CareTeamComponent();
            CareTeamComponent.SequenceElement = new PositiveInt(1);
            CareTeamComponent.Provider = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e");  //"Practitioner/example-01"
            CareTeamComponent.Role = new CodeableConcept("http://snomed.info/sct", "223366009", "Healthcare professional (occupation)", "Healthcare professional (occupation)");
            CareTeamComponent.Qualification = new CodeableConcept("http://snomed.info/sct", "394658006", "Clinical specialty (qualifier value)", "Clinical specialty (qualifier value)");
            claim.CareTeam.Add(CareTeamComponent);

            // set supportingInfo - Supporting information
            Claim.SupportingInformationComponent SupportingInformationComponent = new Claim.SupportingInformationComponent();

            SupportingInformationComponent.Sequence = 1;
            SupportingInformationComponent.Category = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-supportinginfo-category", "INV", "Document Type - Investigation", "Document Type - Investigation");
            SupportingInformationComponent.Code = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");
            var documentReference = new ResourceReference("urn:uuid:e53fa5db-f676-4b16-a273-f4088866314e");
            SupportingInformationComponent.Value = documentReference;
            claim.SupportingInfo.Add(SupportingInformationComponent);

            SupportingInformationComponent.Sequence = 2;
            SupportingInformationComponent.Category = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-supportinginfo-category", "INV", "Document Type - Investigation", "Document Type - Investigation");
            SupportingInformationComponent.Code = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");
            var documentReference1 = new ResourceReference("urn:uuid:514bcad3-7bf0-43a0-b566-e8ecd815dc91");
            SupportingInformationComponent.Value = documentReference1;
            claim.SupportingInfo.Add(SupportingInformationComponent);

            // set diagnosis - Pertinent diagnosis information
            Claim.DiagnosisComponent DiagnosisComponent = new Claim.DiagnosisComponent();
            DiagnosisComponent.Sequence = 1;
            DiagnosisComponent.Diagnosis = new CodeableConcept("http://hl7.org/fhir/sid/icd-10", "I46.9", "Cardiac arrest, unspecified", "Cardiac arrest, unspecified");
            DiagnosisComponent.Type.Add(new CodeableConcept("http://snomed.info/sct", "89100005", "Final diagnosis (discharge) (contextual qualifier) (qualifier value)", "Final diagnosis (discharge) (contextual qualifier) (qualifier value)"));

            claim.Diagnosis.Add(DiagnosisComponent);

            // set Procedure - Clinical procedures performed
            Claim.ProcedureComponent ProcedureComponent = new Claim.ProcedureComponent();
            ProcedureComponent.Sequence = 1;
            ProcedureComponent.Procedure = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");
            claim.Procedure.Add(ProcedureComponent);
            ProcedureComponent.Sequence = 2;
            ProcedureComponent.Procedure = new CodeableConcept("http://snomed.info/sct", "418285008", "Angioplasty of blood vessel", "Angioplasty of blood vessel");
            claim.Procedure.Add(ProcedureComponent);

            // set Insurance - Patient insurance information
            Claim.InsuranceComponent InsuranceComponent = new Claim.InsuranceComponent();
            InsuranceComponent.Sequence = 1;
            InsuranceComponent.Focal = true;
            InsuranceComponent.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");        // 	  "Coverage/example-02"
            IEnumerable<string> m_oEnum = new string[] { "123456" };
            InsuranceComponent.PreAuthRef = m_oEnum;
            claim.Insurance.Add(InsuranceComponent);

            // set Item - Product or service provided
            Claim.ItemComponent item1 = new Claim.ItemComponent();
            item1.Sequence = 1;
            item1.CareTeamSequenceElement.Add(new PositiveInt(1));
            item1.ProductOrService = new CodeableConcept("http://snomed.info/sct", "77343006", "Angiography", "Angiography");

            Money money = new Money();
            money.Currency = Money.Currencies.INR;
            money.Value = (decimal)10000.00;
            item1.UnitPrice = money;
            item1.Net = money;
         

            Claim.ItemComponent item2 = new Claim.ItemComponent();
            item2.Sequence = 2;
            item2.CareTeamSequenceElement.Add(new PositiveInt(1));
            item2.ProductOrService = new CodeableConcept("http://snomed.info/sct", "418285008", "Angioplasty of blood vessel", "Angioplasty of blood vessel");
            Money money1 = new Money();
            money1.Currency = Money.Currencies.INR;
            money1.Value = (decimal)10000;
            item2.UnitPrice = money1;
            item2.Net = money1;         

            Claim.ItemComponent item3 = new Claim.ItemComponent();
            item3.Sequence = 3;
            item3.CareTeamSequenceElement.Add(new PositiveInt(1));
            item3.ProductOrService = new CodeableConcept("http://snomed.info/sct", "309904001", "Intensive care unit", "Intensive care unit");
            Quantity q3 = new Quantity();
            q3.Value = 10;
            q3.Unit = "day";
            item3.Quantity = q3;
            Money money3 = new Money();
            money3.Currency = Money.Currencies.INR;
            money3.Value = (decimal)2000;
            item3.UnitPrice = money3;

            Claim.ItemComponent item4 = new Claim.ItemComponent();
            item4.Sequence = 4;
            item4.CareTeamSequenceElement.Add(new PositiveInt(1));
            item4.ProductOrService = new CodeableConcept("http://snomed.info/sct", "319775004", "Aspirin 75 mg oral tablet", "Aspirin 75 mg oral tablet");

            Money money4 = new Money();
            money4.Currency = Money.Currencies.INR;
            money4.Value = (decimal)100;
            item4.UnitPrice = money4;
            item4.Net = money4;

            claim.Item.Add(item1);
            claim.Item.Add(item2);
            claim.Item.Add(item3);
            claim.Item.Add(item4);

            Money money5 = new Money();
            money5.Currency = Money.Currencies.INR;
            money5.Value = (decimal)90100;

            claim.Total = money5;

            return claim;
        }      

        // populate CliamResponse
        public static ClaimResponse populateClaimResponse(Use claimUse)
        {
            // set Meta - Metadata about the resource
            ClaimResponse claimresponce = new ClaimResponse()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ClaimResponse",
                    },
                },
            };

            // set Id - Logical id of this artifact
            claimresponce.Id = "ClaimResponseBundle-01";       
 
            // set Status - active | cancelled | draft | entered-in-error
            claimresponce.Status = FinancialResourceStatusCodes.Active;

            // set Type - Category or discipline
            claimresponce.Type = new CodeableConcept("http://snomed.info/sct", "710967003", "Management of health status after discharge from hospital (procedure)", "Management of health status after discharge from hospital (procedure)");

            //set Sub-Type
            claimresponce.SubType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/ex-claimsubtype", "emergency", "Emergency Claim", "Emergency Claim");
            
            // set Use - claim | preauthorization | predetermination
            switch (claimUse)
            {
                case Use.Claim:
                    claimresponce.Use = claimUse;
                    break;

                case Use.Preauthorization:
                    claimresponce.Use = claimUse;
                    break;

                case Use.Predetermination:
                    claimresponce.Use = claimUse;
                    break;
                default:
                    break;
            }

            // set Patient - The recipient of the products and services
            claimresponce.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");

            // set Created - Resource creation date
            claimresponce.CreatedElement = new FhirDateTime("2020-07-10");

            // set Insurer - Target
            claimresponce.Insurer = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set requestor
            claimresponce.Requestor = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e");

            // set request
            claimresponce.Request = new ResourceReference("Claim/Claim-01");

            // set outcome
            claimresponce.Outcome = ClaimProcessingCodes.Complete;

            // set disposition
            claimresponce.Disposition = "The claim will be processed within 30 days of this notice.";

            // set PayeeType
            claimresponce.PayeeType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/payeetype", "provider", "Provider", "Provider");
            
            // set Item - Product or service provided

            ClaimResponse.AddedItemComponent item1 = new ClaimResponse.AddedItemComponent();
            IList<int?> itemsequence = new List<int?>();
            itemsequence.Add(1);

            item1.ItemSequence = itemsequence;
            item1.ProductOrService = new CodeableConcept("http://snomed.info/sct", "768839008", "Consultant", "Consultant");
            Money money = new Money();
            money.Currency = Money.Currencies.INR;
            money.Value = (decimal)500.00;
            item1.UnitPrice = money;
            item1.Net = money;

            ClaimResponse.AdjudicationComponent AdjudicationComponent = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent.Amount = money;
            AdjudicationComponent.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");

            item1.Adjudication.Add(AdjudicationComponent);

            ClaimResponse.AddedItemComponent item2 = new ClaimResponse.AddedItemComponent();
            itemsequence.Add(2);
            item2.ItemSequence = itemsequence;
            item2.ProductOrService = new CodeableConcept("http://snomed.info/sct", "768839008", "Metformin hydrochloride 500 mg prolonged-release oral tablet", "Metformin hydrochloride 500 mg prolonged-release oral tablet");
            Money money1 = new Money();
            money1.Currency = Money.Currencies.INR;
            money1.Value = (decimal)910.28;
            item2.UnitPrice = money1;
            item2.Net = money1;

            ClaimResponse.AdjudicationComponent AdjudicationComponent1 = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent1.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent1.Amount = money1;
            AdjudicationComponent1.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");

            item2.Adjudication.Add(AdjudicationComponent1);

            claimresponce.AddItem.Add(item1);
            claimresponce.AddItem.Add(item2);

            // set total
            ClaimResponse.TotalComponent total = new ClaimResponse.TotalComponent();
            total.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "submitted", "submitted");

            Money money3 = new Money();
            money3.Currency = Money.Currencies.INR;
            money3.Value = (decimal)1410.28;
            total.Amount = money3;

            claimresponce.Total.Add(total);

            // set Insurance
            ClaimResponse.InsuranceComponent insurancecomponant = new ClaimResponse.InsuranceComponent();
            insurancecomponant.Sequence = 1;
            insurancecomponant.Focal = true;
            insurancecomponant.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");
            claimresponce.Insurance.Add(insurancecomponant);
  
            return claimresponce;
        }               

        // populate CoverageEligibilityRequest
        public static CoverageEligibilityRequest populateCoverageEligibilityRequest(CoverageEligibilityRequest.EligibilityRequestPurpose CoverageEligibilityRequestPurpose)
        {

            // set Meta - Metadata about the resource
            CoverageEligibilityRequest coverageEligibilityRequest = new CoverageEligibilityRequest()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/CoverageEligibilityRequest",
                    },
                },
            };

            // set Id - Logical id of this artifact
            coverageEligibilityRequest.Id = "78787db5-bcf2-43a2-a01a-e843fd08bfe7";



            // Set identifier - Business Identifier for coverage eligiblity request
            Identifier identifier = new Identifier();

            identifier.System = "https://irdai.gov.in";
            identifier.Value = "123456";
            coverageEligibilityRequest.Identifier.Add(identifier);

            // set status - active | cancelled | draft | entered-in-error
            coverageEligibilityRequest.Status = FinancialResourceStatusCodes.Active;

            // set priority - Desired processing priority
            coverageEligibilityRequest.Priority = new CodeableConcept("http://terminology.hl7.org/CodeSystem/processpriority", "normal", "Normal");

            IList<CoverageEligibilityRequest.EligibilityRequestPurpose?> itemsequence = new List<CoverageEligibilityRequest.EligibilityRequestPurpose?>();
            itemsequence.Add(CoverageEligibilityRequestPurpose);

            coverageEligibilityRequest.Purpose = itemsequence;

            // set patient - Intended recipient of products and services
            coverageEligibilityRequest.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");

            // set created - Creation date
            coverageEligibilityRequest.CreatedElement = new FhirDateTime("2020-07-10");


            // set enterer - Author
            coverageEligibilityRequest.Enterer = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e");

            // set provider - Party responsible for the request
            coverageEligibilityRequest.Provider = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e");

            // set insurer - Coverage issuer
            coverageEligibilityRequest.Insurer = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set facility - Servicing facility
            coverageEligibilityRequest.Facility = new ResourceReference("urn:uuid:1cb884ad-0df4-4c35-ae40-4764895c84c6");

            // set insurance - Patient insurance information
            CoverageEligibilityRequest.InsuranceComponent insurancecomponant = new CoverageEligibilityRequest.InsuranceComponent();
            insurancecomponant.Focal = true;
            insurancecomponant.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");
            coverageEligibilityRequest.Insurance.Add(insurancecomponant);
            
            // set Item - Item to be evaluated for eligibiity

            CoverageEligibilityRequest.DetailsComponent item1 = new CoverageEligibilityRequest.DetailsComponent();
            item1.ProductOrService = new CodeableConcept("http://snomed.info/sct", "768839008", "Consultant", "Consultant");
            CoverageEligibilityRequest.DiagnosisComponent dignosiscomponant = new CoverageEligibilityRequest.DiagnosisComponent();
            dignosiscomponant.Diagnosis = new CodeableConcept("http://hl7.org/fhir/sid/icd-10", "E11.9", "Type 2 diabetes mellitus : Without complications", "Type 2 diabetes mellitus : Without complications");
            item1.Diagnosis.Add(dignosiscomponant);



            CoverageEligibilityRequest.DetailsComponent item2 = new CoverageEligibilityRequest.DetailsComponent();
            item2.ProductOrService = new CodeableConcept("http://snomed.info/sct", "765507008", "Product containing precisely metformin hydrochloride 500 milligram/1 each prolonged-release oral tablet (clinical drug)", "Metformin hydrochloride 500 mg prolonged-release oral tablet");
            CoverageEligibilityRequest.DiagnosisComponent dignosiscomponant1 = new CoverageEligibilityRequest.DiagnosisComponent();
            dignosiscomponant1.Diagnosis = new CodeableConcept("http://hl7.org/fhir/sid/icd-10", "E11.9", "Type 2 diabetes mellitus : Without complications", "Type 2 diabetes mellitus : Without complications");
            item2.Diagnosis.Add(dignosiscomponant1);

            coverageEligibilityRequest.Item.Add(item1);
            coverageEligibilityRequest.Item.Add(item2);

            return coverageEligibilityRequest;
        }

        // populate coverageEligibilityResponse
        public static CoverageEligibilityResponse populateCoverageEligiblityResponse(CoverageEligibilityResponse.EligibilityResponsePurpose CoverageEligibilityResponsePurpose)
        {

            CoverageEligibilityResponse coverageEligiblityResponse = new CoverageEligibilityResponse()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/CoverageEligibilityResponse",
                    },
                },
            };

            // set id
            coverageEligiblityResponse.Id = "9310eab0-febd-42c3-8590-2f03e6c7dfca";

            // set identifier
            Identifier identifier = new Identifier();
            identifier.System = "http://hip.in";
            identifier.Value = "bc3c6c57-2053-4d0e-ac40-139ccccff645";
            coverageEligiblityResponse.Identifier.Add(identifier);

            // set status
            coverageEligiblityResponse.Status = FinancialResourceStatusCodes.Active;

            // set purpose
            IList<CoverageEligibilityResponse.EligibilityResponsePurpose?> itemsequence = new List<CoverageEligibilityResponse.EligibilityResponsePurpose?>();
            itemsequence.Add(CoverageEligibilityResponsePurpose);
            coverageEligiblityResponse.Purpose = itemsequence;

            // set patient
            coverageEligiblityResponse.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");

            // set created date
            coverageEligiblityResponse.CreatedElement = new FhirDateTime("2020-07-10");

            // set requestor
            coverageEligiblityResponse.Requestor = new ResourceReference("urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e");

            // set request
            coverageEligiblityResponse.Request = new ResourceReference("CoverageEligibilityRequest/CoverageEligibilityRequest-01");

            // set outcome

            coverageEligiblityResponse.Outcome = ClaimProcessingCodes.Complete;

            // set disposition

            coverageEligiblityResponse.Disposition = "Policy is currently in-force.";

            // set insurer
            coverageEligiblityResponse.Insurer = new ResourceReference("Organization/Organizatione-01");

            // set insurance
            CoverageEligibilityResponse.InsuranceComponent insurance = new CoverageEligibilityResponse.InsuranceComponent();
            insurance.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");
            insurance.Inforce = true;
            insurance.BenefitPeriod = new Period(new FhirDateTime("2022-02-27"), new FhirDateTime("2023-02-27"));

            // set Item
            CoverageEligibilityResponse.ItemsComponent item1 = new CoverageEligibilityResponse.ItemsComponent();
            item1.ProductOrService = new CodeableConcept("http://snomed.info/sct", "768839008", "Consultant", "Consultant");


            CoverageEligibilityResponse.ItemsComponent item2 = new CoverageEligibilityResponse.ItemsComponent();
            item2.ProductOrService = new CodeableConcept("http://snomed.info/sct", "765507008", "Product containing precisely metformin hydrochloride 500 milligram/1 each prolonged-release oral tablet (clinical drug)", "Metformin hydrochloride 500 mg prolonged-release oral tablet");

            insurance.Item.Add(item1);
            insurance.Item.Add(item2);

            coverageEligiblityResponse.Insurance.Add(insurance);

            return coverageEligiblityResponse;

        }
        
        //populate the Task resource  
        public static Task populateTask()
        {
            // set Meta - Metadata about the resource
            Task task = new Task()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Task",
                    },
                },
            };
          

            // set Id - Logical id of this artifact
            task.Id = "a8f27682-676d-4c2b-8af6-0540721311a0";           

            task.Status = Task.TaskStatus.Requested;
            task.Intent = Task.TaskIntent.Order;

            // set code
            task.Code = new CodeableConcept("http://terminology.hl7.org/CodeSystem/financialtaskcode", "poll", "poll", "poll");

            //set description 
            task.Description = "Please Provide the diagnosis report of the Patient for identifying the disease";
            task.AuthoredOn = "2023-12-08T08:25:05+10:00";

            // set requestor 
            task.Requester = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set owner 
            task.Owner = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");

            //set input
            Task.ParameterComponent parameterComponent = new Task.ParameterComponent();
            parameterComponent.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/financialtaskinputtype", "include", "include", "include");
            parameterComponent.Value = new ResourceReference("urn:uuid:7061d007-2ac3-4cf2-b117-a2043f985c45");
            task.Input.Add(parameterComponent);                         

            return task;
        }

        //populate the Task resource  
        public static Task populateSecondTask()
        {
            // set Meta - Metadata about the resource
            Task task = new  Task()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Task",
                    },
                },
            };

            // set Id - Logical id of this artifact
            task.Id = "75107ae5-8514-482a-bc38-88f6c82ccac1";           

            task.Status = Task.TaskStatus.Requested;
            task.Intent = Task.TaskIntent.Order;

            // set code
            task.Code = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-task-codes", "deliver", "deliver", "deliver");

            //set description 
            task.Description ="payment status Check";
            task.AuthoredOn = "2023-12-08T08:25:05+10:00";

            // set requestor 
            task.Requester = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set owner 
            task.Owner = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            //set input
            Task.ParameterComponent parameterComponent = new Task.ParameterComponent();
            parameterComponent.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/financialtaskcode", "status", "Status check", "status");

            parameterComponent.Value = new ResourceReference("urn:uuid:86e38f6d-4af6-4d24-aa7f-8bd774bf5080");
            task.Input.Add(parameterComponent);

            return task;
        }       

        //populate the Task resource   -- Payment Notice Responce 
        public static Hl7.Fhir.Model.Task populateTask_PaymentNoticeResponce()
        {
            // set Meta - Metadata about the resource
            Hl7.Fhir.Model.Task task = new Hl7.Fhir.Model.Task()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Task",
                    },
                },
            };

            // set Id - Logical id of this artifact
            task.Id = "a5185031-666a-4f4c-9d62-2c1993e9a11c";

            task.Status = Task.TaskStatus.Completed;
            task.Intent = Task.TaskIntent.Order;

            // set code
            task.Code = new CodeableConcept("http://terminology.hl7.org/CodeSystem/financialtaskcode", "status", "Status check", "status");

            //set description 
            task.Description = "response for payment status Check";

            //set focus  
            task.Focus = new ResourceReference("urn:uuid:4776dbdf-d596-4cd1-9966-9d44ae9dec0b");

            task.AuthoredOn = "2023-12-08T08:25:05+10:00";

            // set requestor 
            task.Requester = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");

            // set owner 
            task.Owner = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            //set Output
            Task.OutputComponent outputComponent = new Task.OutputComponent();            
            outputComponent.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/financialtaskcode", "status", "Status check", "status");
            outputComponent.Value = new FhirString("Recieved");
            task.Output.Add(outputComponent);

            return task;
        }

        //populate the Task resource  -- Reprocess Request 
        public static Hl7.Fhir.Model.Task populateTaskReprocessRequest()
        {
            // set Meta - Metadata about the resource
            Hl7.Fhir.Model.Task task = new Hl7.Fhir.Model.Task()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Task",
                    },
                },
            };

            // set Id - Logical id of this artifact
            task.Id = "23385f90-aa41-4e82-b212-d7a6191737a4";                      

            task.Status = Task.TaskStatus.Requested;
            task.Intent = Task.TaskIntent.Order;

            // set code
            task.Code = new CodeableConcept("http://terminology.hl7.org/CodeSystem/financialtaskcode", "reprocess", "reprocess", "reprocess");

            //set description 
            task.Description = "Request for reprocesing the claim with number - 761234674365";
            task.AuthoredOn = "2023-12-08T08:25:05+10:00";

            // set requestor 
            task.Requester = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");

            // set owner 
            task.Owner = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            //set input
            Task.ParameterComponent parameterComponent = new Task.ParameterComponent();
            parameterComponent.Type = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-task-input-type-code", "claimNumber", "claimNumber", "claimNumber");
            parameterComponent.Value = new FhirString("761234674365");
            task.Input.Add(parameterComponent);

            return task;
        }
        
        //populate the Task resource  -- Search Request 
        public static Hl7.Fhir.Model.Task populateTaskSearchRequest()
        {
            // set Meta - Metadata about the resource
            Hl7.Fhir.Model.Task task = new Hl7.Fhir.Model.Task()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Task",
                    },
                },
            };

            // set Id - Logical id of this artifact
            task.Id = "fd549b56-c569-4a16-ab17-b52744230d73";
        
            task.Status = Task.TaskStatus.Requested;
            task.Intent = Task.TaskIntent.Order;

            // set code
            task.Code = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-task-codes", "search", "Search", "search");

            //set description 
            task.Description = "Request for Searching the claim with number - 761234674365";
            task.AuthoredOn = "2023-12-08T08:25:05+10:00";

            // set requestor 
            task.Requester = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");

            // set owner 
            task.Owner = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            //set input
            Task.ParameterComponent parameterComponent = new Task.ParameterComponent();
            parameterComponent.Type = new CodeableConcept("https://nrces.in/ndhm/fhir/r4/CodeSystem/ndhm-task-input-type-code", "claimNumber", "claimNumber", "claimNumber");
            parameterComponent.Value = new FhirString("761234674365");
            task.Input.Add(parameterComponent);

            return task;
        }

        //populate the Task resource  -- Search Responce
        public static Hl7.Fhir.Model.Task populateTaskSearchResponse()
        {
            // set Meta - Metadata about the resource
            Hl7.Fhir.Model.Task task = new Hl7.Fhir.Model.Task()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Task",
                    },
                },
            };

            // set Id - Logical id of this artifact
            task.Id = "36dc3058-fdf9-4765-9552-06f0c2a0c635";                     

            task.Status = Task.TaskStatus.Requested;
            task.Intent = Task.TaskIntent.Order;

            // set code
            task.Code = new CodeableConcept("http://terminology.hl7.org/CodeSystem/financialtaskcode", "poll", "poll", "poll");

            //set description 
            task.Description = "search result for the Claim No. - 761234674365";
            task.AuthoredOn = "2023-12-08T08:25:05+10:00";

            // set requestor 
            task.Requester = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set owner 
            task.Owner = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");

            //set Output
            Task.OutputComponent outputComponent = new Task.OutputComponent ();
            outputComponent.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/financialtaskinputtype", "origresponse", "Original Response", "origresponse");
            outputComponent.Value = new ResourceReference("urn:uuid:03acb7b0-3833-45a0-9885-ad35940a3458");
            task.Output.Add(outputComponent);
           

            return task;
        }


        //populate the Task resource  -- Reprocess Responce
        public static Hl7.Fhir.Model.Task populateTask_ReprocessResponse()
        {
            // set Meta - Metadata about the resource
            Hl7.Fhir.Model.Task task = new Hl7.Fhir.Model.Task()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Task",
                    },
                },
            };

            // set Id - Logical id of this artifact
            task.Id = "9d04af7b-9d68-4bad-bf10-99615df16dde";            

            task.Status = Task.TaskStatus.Completed;
            task.Intent = Task.TaskIntent.Order;

            // set code
            task.Code = new CodeableConcept("http://hl7.org/fhir/CodeSystem/task-code", "approve", "Activate/approve the focal resource", "approve");

            //set description 
            task.Description = "Start processing of Claim No. - 761234674365";
            task.AuthoredOn = "2023-12-08T08:25:05+10:00";

            // set requestor 
            task.Requester = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set owner 
            task.Owner = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");

            //set Output
            Task.OutputComponent outputComponent = new Task.OutputComponent();
            outputComponent.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/financialtaskinputtype", "origresponse", "Original Response", "origresponse");
            outputComponent.Value = new ResourceReference("urn:uuid:03acb7b0-3833-45a0-9885-ad35940a3458");
            task.Output.Add(outputComponent);

           


            return task;
        }

        //populate the CommunicationRequest resource  
        public static CommunicationRequest populateCommunicationRequest()
        {
            // set Meta - Metadata about the resource
            CommunicationRequest communicationrequest = new CommunicationRequest()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/CommunicationRequest",
                    },
                },
            };

            // set Id - Logical id of this artifact

            communicationrequest.Id = "7061d007-2ac3-4cf2-b117-a2043f985c45";
            
            // set identifier
            Identifier identifier = new Identifier();
            identifier.System = "http://irdai.in";
            identifier.Value = "4524657454";
            communicationrequest.Identifier.Add(identifier);

            // set basedon
            communicationrequest.BasedOn.Add(new ResourceReference("urn:uuid:760ec93e-7ec8-4e82-b8a3-fe6512fccd8b"));

            //set status
            communicationrequest.Status = RequestStatus.Active;

            //set category
            communicationrequest.Category.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/communication-category", "alert", "alert", "alert"));

            //set Priority
            communicationrequest.Priority = RequestPriority.Routine;

            // set payload
            CommunicationRequest.PayloadComponent payloadComponent = new CommunicationRequest.PayloadComponent();
            var contentstring = new FhirString("Please provide the Angeography report to support your Claim# DEF5647.");           
            payloadComponent.Content = contentstring;
            communicationrequest.Payload.Add(payloadComponent);

            // set authoredon 
            communicationrequest.AuthoredOn = "2023-12-08T08:25:05+10:00";

            // set requestor 
            communicationrequest.Requester = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set owner 
            communicationrequest.Recipient.Add(new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f"));

            //set sender
            communicationrequest.Sender = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");
            

            return communicationrequest;
        }

        //populate the Communication resource  
        public static Communication populateCommunication()
        {
            // set Meta - Metadata about the resource
            Communication communication = new Communication()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/Communication",
                    },
                },
            };

            // set Id - Logical id of this artifact
            communication.Id = "7c129ea5-e402-4162-9045-0ae659898dce"; 

            // set identifier
            Identifier identifier = new Identifier();
            identifier.System = "http://irdai.in";
            identifier.Value = "4524657454";
            communication.Identifier.Add(identifier);

            // set basedon

            communication.BasedOn.Add(new ResourceReference("urn:uuid:7061d007-2ac3-4cf2-b117-a2043f985c45"));

            //set status
            communication.Status = EventStatus.Completed;
            
            //set category
            communication.Category.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/communication-category", "notification", "notification", "notification"));

            //set Priority
            communication.Priority = RequestPriority.Routine;

            // set recipient 
            communication.Recipient.Add(new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f"));
            
            // set sender  
            communication.Sender = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");

            // set payload
            Communication.PayloadComponent payloadComponent = new Communication.PayloadComponent();
            Attachment attachment = new Attachment();
            attachment.ContentType = "application/pdf";
            attachment.Language = "en-IN";
            byte[] bytes = Encoding.ASCII.GetBytes("IDc4NTkxPj4NCnN0YXJ0eHJlZg0KODA2MTQNCiUlRU9G");           
            attachment.DataElement = new Hl7.Fhir.Model.Base64Binary(bytes);
            attachment.Title = "Angeography report PDF";            
            attachment.CreationElement = new FhirDateTime("2023-12-08T14:58:58.181+05:30");

            payloadComponent.Content = attachment;

            communication.Payload.Add(payloadComponent);

            return communication;
        }

        //populate the PaymentNotice resource  
        public static PaymentNotice populatePaymentNotice()
        {
            // set Meta - Metadata about the resource
            PaymentNotice paymentNotice = new PaymentNotice()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                        "https://nrces.in/ndhm/fhir/r4/StructureDefinition/PaymentNotice",
                    },
                },
            };

            // set Id - Logical id of this artifact
            paymentNotice.Id = "86e38f6d-4af6-4d24-aa7f-8bd774bf5080";
           
            // set identifier
            Identifier identifier = new Identifier();
            identifier.System = "http://benefitsinc.com/paymentnotice";
            identifier.Value = "776543";
            paymentNotice.Identifier.Add(identifier);


            // set status 
            paymentNotice.Status = FinancialResourceStatusCodes.Active;

            // set request
            paymentNotice.Request= new ResourceReference("urn:uuid:4776dbdf-d596-4cd1-9966-9d44ae9dec0b");

            // set created 
            paymentNotice.Created = "2024-01-04T14:58:58.181+05:30";

            //set payment
            paymentNotice.Payment = new ResourceReference("urn:uuid:e3fb872f-596c-4d6c-8e99-b246f4f10690");

            // set payment date
            paymentNotice.PaymentDate ="2023-01-04";

            // set recipient 
            paymentNotice.Recipient = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            paymentNotice.Recipient.Identifier = new Identifier();
            paymentNotice.Recipient.Identifier.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "PRN", "Provider number", "Provider number");
            paymentNotice.Recipient.Identifier.System = "https://facility.ndhm.gov.in";
            paymentNotice.Recipient.Identifier.Value = "45675454";

            // set amount   
            Money amount = new Money();
            amount.Value = 90100;
            amount.Currency = Money.Currencies.INR;
            paymentNotice.Amount = amount;

            // set payment status
            paymentNotice.PaymentStatus = new CodeableConcept("http://terminology.hl7.org/CodeSystem/paymentstatus", "paid", "paid", "paid");                        

            return paymentNotice;
        }

        //populate the PaymentReconciliation resource  
        public static PaymentReconciliation populatePaymentReconciliation()
        {
            // set Meta - Metadata about the resource
            PaymentReconciliation paymentReconciliation = new PaymentReconciliation()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                        "https://nrces.in/ndhm/fhir/r4/StructureDefinition/PaymentReconciliation",
                    },
                },
            };

            // set Id - Logical id of this artifact
            paymentReconciliation.Id = "e3fb872f-596c-4d6c-8e99-b246f4f10690";            

            // set identifier
            Identifier identifier = new Identifier();
            identifier.System ="http://www.BenefitsInc.com/fhir/enrollmentresponse";
            identifier.Value = "781234";
            paymentReconciliation.Identifier.Add(identifier);

            // set status 
            paymentReconciliation.Status = FinancialResourceStatusCodes.Active;

            // set period
            paymentReconciliation.Period = new Period();
            paymentReconciliation.Period.Start = "2023-12-30T11:01:00+05:00";
            paymentReconciliation.Period.End = "2024-01-09T11:01:00+05:00";

            // set created 
            paymentReconciliation.Created = "2024-01-05";

            // set payment issuer 
            paymentReconciliation.PaymentIssuer  = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set payment requestor  
            paymentReconciliation.Requestor = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");

            // set payment outcome
            paymentReconciliation.Outcome = ClaimProcessingCodes.Complete;

            // set payment disposition
            paymentReconciliation.Disposition = "2023 December month settlement.";

            // set payment date
            paymentReconciliation.PaymentDate = "2023-01-04";          

            // set amount   
            Money amount = new Money();
            amount.Value = 90100;
            amount.Currency = Money.Currencies.INR;
            paymentReconciliation.PaymentAmount = amount;

            // set payment identifier
            Identifier identifier1 = new Identifier();
            identifier1.System = "https://services.india.gov.in/service/detail/transictionid";
            identifier1.Value = "1012424354354345";

            paymentReconciliation.PaymentIdentifier = identifier1;

            return paymentReconciliation;
        }


        // populate CliamResponse -- enhancement 
        public static ClaimResponse populateClaimResponseenhancement()
        {
            // set Meta - Metadata about the resource
            ClaimResponse claimresponce = new ClaimResponse()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ClaimResponse",
                    },
                },
            };

            claimresponce.Id = "84a452d5-9ae6-4baf-854c-88df16bcdcf5";            

            // set Id - Logical id of this artifact
            claimresponce.Id = "ClaimResponse-enhancement-01";

            // set Status - active | cancelled | draft | entered-in-error
            claimresponce.Status = FinancialResourceStatusCodes.Active;

            // set Type - Category or discipline
            claimresponce.Type = new CodeableConcept("http://snomed.info/sct", "737481003", "Inpatient care management (procedure)", "Inpatient care management (procedure)");

            //set Sub-Type
            claimresponce.SubType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/ex-claimsubtype", "emergency", "Emergency Claim");

            // set Use - claim | preauthorization | predetermination
            claimresponce.Use = Use.Preauthorization;

            // set Patient - The recipient of the products and services
            claimresponce.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");

            // set Created - Resource creation date
            claimresponce.Created = "2023-12-30T15:32:26.605+05:30";

            // set Insurer - Target
            claimresponce.Insurer = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set Requestor 
            claimresponce.Requestor = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");

            // set request
            claimresponce.Request = new ResourceReference("urn:uuid:7aace234-5172-4126-a907-ace8745bd1a5");

            // set outcome
            claimresponce.Outcome = ClaimProcessingCodes.Complete;

            // set disposition
            claimresponce.Disposition = "The enclosed services are authorized for your provision within 30 days of this notice.";

            // set preAuthref
            claimresponce.PreAuthRef = "123456";

            // set PayeeType
            claimresponce.PayeeType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/payeetype", "provider", "Provider");

            // set Item - Product or service provided             
            ClaimResponse.ItemComponent item1 = new ClaimResponse.ItemComponent();
        
            item1.ItemSequence = 1;         

            ClaimResponse.AdjudicationComponent AdjudicationComponent = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            Money money = new Money();
            money.Currency = Money.Currencies.INR;
            money.Value = (decimal)10000.00;
            AdjudicationComponent.Amount = money;
            item1.Adjudication.Add(AdjudicationComponent);


            ClaimResponse.ItemComponent item2 = new ClaimResponse.ItemComponent();            
            item2.ItemSequence = 2;           
            Money money1 = new Money();
            money1.Currency = Money.Currencies.INR;
            money1.Value = (decimal)10000;            
            ClaimResponse.AdjudicationComponent AdjudicationComponent1 = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent1.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent1.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent1.Amount = money1;
            item2.Adjudication.Add(AdjudicationComponent1);          
            claimresponce.Item.Add(item1);
            claimresponce.Item.Add(item2);           

            // set total
            ClaimResponse.TotalComponent total = new ClaimResponse.TotalComponent();
            total.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "submitted");

            Money moneytotal = new Money();
            moneytotal.Currency = Money.Currencies.INR;
            moneytotal.Value = (decimal)20000;
            total.Amount = moneytotal;

            claimresponce.Total.Add(total);

            // set Insurance
            ClaimResponse.InsuranceComponent insurancecomponant = new ClaimResponse.InsuranceComponent();
            insurancecomponant.Sequence = 1;
            insurancecomponant.Focal = true;
            insurancecomponant.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");
            claimresponce.Insurance.Add(insurancecomponant);

            return claimresponce;
        }

        // populate CliamResponse -- preauth 
        public static ClaimResponse populateClaimResponsepreauth()
        {
            // set Meta - Metadata about the resource
            ClaimResponse claimresponce = new ClaimResponse()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ClaimResponse",
                    },
                },
            };

            claimresponce.Id = "e808bcbe-9b25-4167-b914-7b6dc7295bba";             

            // set Id - Logical id of this artifact
            claimresponce.Id = "ClaimResponse-preauth-01";

            // set Status - active | cancelled | draft | entered-in-error
            claimresponce.Status = FinancialResourceStatusCodes.Active;

            // set Type - Category or discipline
            claimresponce.Type = new CodeableConcept("http://snomed.info/sct", "737481003", "Inpatient care management (procedure)", "Inpatient care management (procedure)");

            //set Sub-Type
            claimresponce.SubType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/ex-claimsubtype", "emergency", "Emergency Claim");

            // set Use - claim | preauthorization | predetermination
            claimresponce.Use = Use.Preauthorization;

            // set Patient - The recipient of the products and services
            claimresponce.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");

            // set Created - Resource creation date
            claimresponce.Created = "2023-12-30T15:32:26.605+05:30";

            // set Insurer - Target
            claimresponce.Insurer = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set Requestor 
            claimresponce.Requestor = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");

            // set request
            claimresponce.Request = new ResourceReference("urn:uuid:760ec93e-7ec8-4e82-b8a3-fe6512fccd8b");

            // set outcome
            claimresponce.Outcome = ClaimProcessingCodes.Complete;

            // set disposition
            claimresponce.Disposition = "The enclosed services are authorized for your provision within 30 days of this notice.";

            // set preAuthref
            claimresponce.PreAuthRef = "123456";

            // set PayeeType
            claimresponce.PayeeType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/payeetype", "provider", "Provider");

            // set Item - Product or service provided           
            ClaimResponse.ItemComponent item1 = new ClaimResponse.ItemComponent();         

            item1.ItemSequence = 1;           

            Money money = new Money();
            money.Currency = Money.Currencies.INR;
            money.Value = (decimal)10000.00;            

            ClaimResponse.AdjudicationComponent AdjudicationComponent = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent.Amount = money;
            item1.Adjudication.Add(AdjudicationComponent);


            ClaimResponse.ItemComponent item2 = new ClaimResponse.ItemComponent();
            item2.ItemSequence = 3;           
            Money money1 = new Money();
            money1.Currency = Money.Currencies.INR;
            money1.Value = (decimal)40000;

            ClaimResponse.AdjudicationComponent AdjudicationComponent1 = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent1.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent1.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent1.Amount = money1;
            item2.Adjudication.Add(AdjudicationComponent1);

            ClaimResponse.ItemComponent item3 = new ClaimResponse.ItemComponent();
          
            item3.ItemSequence = 3;        

            Money money3 = new Money();
            money3.Currency = Money.Currencies.INR;
            money3.Value = (decimal)20000;        

            ClaimResponse.AdjudicationComponent AdjudicationComponent3 = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent3.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent3.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent3.Amount = money3;
            item3.Adjudication.Add(AdjudicationComponent3);


            ClaimResponse.ItemComponent item4 = new ClaimResponse.ItemComponent();
            
            item4.ItemSequence = 4;        
            Money money4 = new Money();
            money4.Currency = Money.Currencies.INR;
            money4.Value = (decimal)100;
           
            ClaimResponse.AdjudicationComponent AdjudicationComponent4 = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent4.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent4.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent4.Amount = money4;
            item4.Adjudication.Add(AdjudicationComponent4);

            claimresponce.Item.Add(item1);
            claimresponce.Item.Add(item2);
            claimresponce.Item.Add(item3);
            claimresponce.Item.Add(item4);

            // set total
            ClaimResponse.TotalComponent total = new ClaimResponse.TotalComponent();
            total.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "submitted");

            Money moneytotal = new Money();
            moneytotal.Currency = Money.Currencies.INR;
            moneytotal.Value = (decimal)70100;
            total.Amount = moneytotal;

            claimresponce.Total.Add(total);

            // set Insurance
            ClaimResponse.InsuranceComponent insurancecomponant = new ClaimResponse.InsuranceComponent();
            insurancecomponant.Sequence = 1;
            insurancecomponant.Focal = true;
            insurancecomponant.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");
            claimresponce.Insurance.Add(insurancecomponant);

            return claimresponce;
        }

        // populate CliamResponse -- predetermination 
        public static ClaimResponse populateClaimResponsepredetermination()
        {
            // set Meta - Metadata about the resource
            ClaimResponse claimresponce = new ClaimResponse()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ClaimResponse",
                    },
                },
            };
            claimresponce.Id = "731b668b-d5b2-43a3-923f-20e284113919";           

            // set Id - Logical id of this artifact
            claimresponce.Id = "ClaimResponse-predetermination-01";

            // set Status - active | cancelled | draft | entered-in-error
            claimresponce.Status = FinancialResourceStatusCodes.Active;

            // set Type - Category or discipline
            claimresponce.Type = new CodeableConcept("http://snomed.info/sct", "737481003", "Inpatient care management (procedure)", "Inpatient care management (procedure)");

            //set Sub-Type
            claimresponce.SubType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/ex-claimsubtype", "emergency", "Emergency Claim");

            // set Use - claim | preauthorization | predetermination
            claimresponce.Use = Use.Predetermination;

            // set Patient - The recipient of the products and services
            claimresponce.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");

            // set Created - Resource creation date
            claimresponce.Created = "2023-12-30T15:32:26.605+05:30";

            // set Insurer - Target
            claimresponce.Insurer = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set Requestor 
            claimresponce.Requestor = new ResourceReference("urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a");

            // set request
            claimresponce.Request = new ResourceReference("urn:uuid:372a5471-1e67-4501-8c29-b20b783ba33e");

            // set outcome
            claimresponce.Outcome = ClaimProcessingCodes.Complete;           

            // set preAuthref
            claimresponce.PreAuthRef = "123456";

            // set PayeeType
            claimresponce.PayeeType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/payeetype", "provider", "Provider");

            // set Item - Product or service provided           
            ClaimResponse.ItemComponent item1 = new ClaimResponse.ItemComponent();
     

            item1.ItemSequence = 1;          
            Money money = new Money();
            money.Currency = Money.Currencies.INR;
            money.Value = (decimal)10000.00;           

            ClaimResponse.AdjudicationComponent AdjudicationComponent = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent.Amount = money;
            item1.Adjudication.Add(AdjudicationComponent);


            ClaimResponse.ItemComponent item2 = new ClaimResponse.ItemComponent();
          
            item2.ItemSequence = 2;           
            Money money1 = new Money();
            money1.Currency = Money.Currencies.INR;
            money1.Value = (decimal)40000;
            
            ClaimResponse.AdjudicationComponent AdjudicationComponent1 = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent1.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent1.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent1.Amount = money1;
            item2.Adjudication.Add(AdjudicationComponent1);

            ClaimResponse.ItemComponent item3 = new ClaimResponse.ItemComponent();
             
            item3.ItemSequence = 3;         
            Money money3 = new Money();
            money3.Currency = Money.Currencies.INR;
            money3.Value = (decimal)20000;
           
            ClaimResponse.AdjudicationComponent AdjudicationComponent3 = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent3.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent3.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent3.Amount = money3;
            item3.Adjudication.Add(AdjudicationComponent3);


            ClaimResponse.ItemComponent item4 = new ClaimResponse.ItemComponent();
       
            item4.ItemSequence = 4;           

            Money money4 = new Money();
            money4.Currency = Money.Currencies.INR;
            money4.Value = (decimal)100;          

            ClaimResponse.AdjudicationComponent AdjudicationComponent4 = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent4.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent4.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent4.Amount = money4;
            item4.Adjudication.Add(AdjudicationComponent4);

            claimresponce.Item.Add(item1);
            claimresponce.Item.Add(item2);
            claimresponce.Item.Add(item3);
            claimresponce.Item.Add(item4);

            // set total
            ClaimResponse.TotalComponent total = new ClaimResponse.TotalComponent();
            total.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "submitted");

            Money moneytotal = new Money();
            moneytotal.Currency = Money.Currencies.INR;
            moneytotal.Value = (decimal)70100;
            total.Amount = moneytotal;

            claimresponce.Total.Add(total);

            // set Insurance
            ClaimResponse.InsuranceComponent insurancecomponant = new ClaimResponse.InsuranceComponent();
            insurancecomponant.Sequence = 1;
            insurancecomponant.Focal = true;
            insurancecomponant.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");
            claimresponce.Insurance.Add(insurancecomponant);

            return claimresponce;
        }

        // populate CliamResponse -- settlement
        public static ClaimResponse populateClaimResponsesettlement()
        {
            // set Meta - Metadata about the resource
            ClaimResponse claimresponce = new ClaimResponse()
            {
                // Set metadata about the resource - Version Id, Lastupdated Date, Profile
                Meta = new Meta()
                {
                    VersionId = "1",
                    Profile = new List<string>()
                    {
                     "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ClaimResponse",
                    },
                },
            }; 
            // set Id - Logical id of this artifact
            claimresponce.Id= "03acb7b0-3833-45a0-9885-ad35940a3458";  

            // set Status - active | cancelled | draft | entered-in-error
            claimresponce.Status = FinancialResourceStatusCodes.Active;

            // set Type - Category or discipline
            claimresponce.Type = new CodeableConcept("http://snomed.info/sct", "737481003", "Inpatient care management (procedure)","Inpatient care management (procedure)");

            //set Sub-Type
            claimresponce.SubType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/ex-claimsubtype", "emergency", "Emergency Claim");

            // set Use - claim | preauthorization | predetermination
            claimresponce.Use = Use.Claim;

            // set Patient - The recipient of the products and services
            claimresponce.Patient = new ResourceReference("urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe");

            // set Created - Resource creation date
            claimresponce.Created  = "2023-12-30T15:32:26.605+05:30";

            // set Insurer - Target
            claimresponce.Insurer = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");

            // set Requestor 
            claimresponce.Requestor = new ResourceReference("urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f");                      

            // set request

            claimresponce.Request = new ResourceReference("urn:uuid:f2413fb3-f688-4400-a67c-f28c1fbe88cb");

            // set outcome
            claimresponce.Outcome = ClaimProcessingCodes.Complete;

            // set disposition
            claimresponce.Disposition = "The claim will be processed within 30 days of this notice.";

            // set preAuthref
            claimresponce.PreAuthRef = "123456";

            // set PayeeType
            claimresponce.PayeeType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/payeetype", "provider", "Provider");

            // set Item - Product or service provided           
            ClaimResponse.ItemComponent item1 = new ClaimResponse.ItemComponent();             

            item1.ItemSequence = 1;
         
            Money money = new Money();
            money.Currency = Money.Currencies.INR;
            money.Value = (decimal)20000.00;
           
            ClaimResponse.AdjudicationComponent AdjudicationComponent = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent.Amount = money;          
            item1.Adjudication.Add(AdjudicationComponent);
            
            ClaimResponse.ItemComponent item2 = new ClaimResponse.ItemComponent();            
            item2.ItemSequence = 2;          
            Money money1 = new Money();
            money1.Currency = Money.Currencies.INR;
            money1.Value = (decimal)50000;           
          
            ClaimResponse.AdjudicationComponent AdjudicationComponent1 = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent1.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent1.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent1.Amount = money1;
            item2.Adjudication.Add(AdjudicationComponent1);

            ClaimResponse.ItemComponent item3 = new ClaimResponse.ItemComponent();           
            item3.ItemSequence = 3;            
        
            Money money3 = new Money();
            money3.Currency = Money.Currencies.INR;
            money3.Value = (decimal)2000;
           
            ClaimResponse.AdjudicationComponent AdjudicationComponent3 = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent3.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent3.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent3.Amount = money3;
            item3.Adjudication.Add(AdjudicationComponent3);

            ClaimResponse.ItemComponent item4 = new ClaimResponse.ItemComponent();          
            item4.ItemSequence = 4;
                       
            Money money4 = new Money();
            money4.Currency = Money.Currencies.INR;
            money4.Value = (decimal)100;
            ClaimResponse.AdjudicationComponent AdjudicationComponent4 = new ClaimResponse.AdjudicationComponent();
            AdjudicationComponent4.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "eligible", "Eligible Amount", "Eligible Amount");
            AdjudicationComponent4.Reason = new CodeableConcept("http://snomed.info/sct", "255334000", "Covered", "Covered");
            AdjudicationComponent4.Amount = money4;
            item4.Adjudication.Add(AdjudicationComponent4);

            claimresponce.Item.Add(item1);
            claimresponce.Item.Add(item2);
            claimresponce.Item.Add(item3);
            claimresponce.Item.Add(item4);

            // set total
            ClaimResponse.TotalComponent total = new ClaimResponse.TotalComponent();
            total.Category = new CodeableConcept("http://terminology.hl7.org/CodeSystem/adjudication", "submitted");

            Money moneytotal = new Money();
            moneytotal.Currency = Money.Currencies.INR;
            moneytotal.Value = (decimal)90100;
            total.Amount = moneytotal;

            claimresponce.Total.Add(total);

            // set Insurance
            ClaimResponse.InsuranceComponent insurancecomponant = new ClaimResponse.InsuranceComponent();
            insurancecomponant.Sequence = 1;
            insurancecomponant.Focal = true;
            insurancecomponant.Coverage = new ResourceReference("urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5");
            claimresponce.Insurance.Add(insurancecomponant);

            return claimresponce;
        }
        #endregion
    }
}
