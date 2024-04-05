using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.IO;
using Hl7.Fhir.Validation;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Source;

namespace FHIR_Profile_Validation
{
    class WellnessRecordSample
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside WellnessRecordSample");
                fnWellnessRecordSample(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("WellnessRecordSample ERROR:---" + e.Message);
            }

        }

        static bool fnWellnessRecordSample(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle WellnessRecordBundle = new Bundle();
                WellnessRecordBundle = populateWellnessRecordBundle();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(WellnessRecordBundle, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated WellnessRecord bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("WellnessRecordBundle.json", WellnessRecordBundle);
                    if (isProfileCreated == false)
                    {
                        Console.WriteLine("Error in Profile File creation");
                    }
                    else
                    {
                        Console.WriteLine("Success");
                    }
                }
                strError_OUT = "";
                return blnReturn;
            }
            catch (Exception ex)
            {
                blnReturn = false;
                strError_OUT = ex.InnerException.ToString();
                return blnReturn;
            }
        }
        static Bundle populateWellnessRecordBundle()
        {
            // Set metadata about the resource            
            Bundle WellnessRecordBundle = new Bundle()
            {
                // Set logical id of this artifact
                Id = "WellnessRecord-01",
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DocumentBundle",
                    },
                    // Set Confidentiality as defined by affinity domain
                    Security = new List<Coding>()
                    {
                        new Coding("http://terminology.hl7.org/CodeSystem/v3-Confidentiality", "V", "very restricted"),
                    }
                },
            };

            // Set version-independent identifier for the Bundle
            Identifier identifier = new Identifier();
            identifier.Value = "305fecc2-4ba2-46cc-9ccd-efa755aff51d";
            identifier.System = "http://hip.in";
            WellnessRecordBundle.Identifier = identifier;

            // Set Bundle Type 
            WellnessRecordBundle.Type = Bundle.BundleType.Document;

            ////// Set Timestamp  
            var dtStr = "2020-07-09T15:32:26.605+05:30";
            WellnessRecordBundle.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:5268c7b1-5b29-44ec-b25f-f6be91e46511";
            bundleEntry1.Resource = ResourcePopulator.populateWellnessRecordCompositionResource();
            WellnessRecordBundle.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147";
            bundleEntry2.Resource = ResourcePopulator.populatePractitionerResource();
            WellnessRecordBundle.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134";
            bundleEntry3.Resource = ResourcePopulator.populatePatientResource();
            WellnessRecordBundle.Entry.Add(bundleEntry3);
            
            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:d28df502-7c86-43a1-a4ec-ecd7f026bd37";                     //Observation/respiratory-rate
            bundleEntry4.Resource = ResourcePopulator.populateRespitaryRateResource();
            WellnessRecordBundle.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:bd809f65-3248-412f-98f4-6d5e38c71833";                           //Observation/heart-rate
            bundleEntry5.Resource = ResourcePopulator.populateHeartRateResource();
            WellnessRecordBundle.Entry.Add(bundleEntry5);

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:1a100486-f1be-47ba-af73-b86b850f2cea";                   //Observation/body-temperature
            bundleEntry6.Resource = ResourcePopulator.populateBodyTemperatureResource();
            WellnessRecordBundle.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:cd3b03d5-e10a-4934-8fe9-37f17bdf458c";                         //Observation/body-height
            bundleEntry7.Resource = ResourcePopulator.populateBodyHeightResource();
            WellnessRecordBundle.Entry.Add(bundleEntry7);

            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:9ef438b3-1b55-44f7-8ae5-879afc7eaafb";                                  //Observation/body-weight
            bundleEntry8.Resource = ResourcePopulator.populateBodyWeightResource();
            WellnessRecordBundle.Entry.Add(bundleEntry8);

            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:86357581-6eb5-43e3-900a-5a729ab2cd90";                                //Observation/bmi
            bundleEntry9.Resource = ResourcePopulator.populateBMIResource();
            WellnessRecordBundle.Entry.Add(bundleEntry9);

            var bundleEntry10 = new Bundle.EntryComponent();
            bundleEntry10.FullUrl = "urn:uuid:b700d75e-d819-45aa-8981-d5941c8abfee";                                  //Observation/blood-pressure
            bundleEntry10.Resource = ResourcePopulator.populateBloodPressureResource();
            WellnessRecordBundle.Entry.Add(bundleEntry10);

            var bundleEntry11 = new Bundle.EntryComponent();
            bundleEntry11.FullUrl = "urn:uuid:42a0955b-b1cb-4727-96d7-b202ff5db03f";                    //Observation/StepCount
            bundleEntry11.Resource = ResourcePopulator.populateStepCountResource();
            WellnessRecordBundle.Entry.Add(bundleEntry11);

            var bundleEntry12 = new Bundle.EntryComponent();
            bundleEntry12.FullUrl = "urn:uuid:9dd4c4e5-554e-4b05-bfc8-6aed5d258bd3";                  //Observation/CaloriesBurned
            bundleEntry12.Resource = ResourcePopulator.populateCaloriesBurnedResource();
            WellnessRecordBundle.Entry.Add(bundleEntry12);

            var bundleEntry13 = new Bundle.EntryComponent();
            bundleEntry13.FullUrl = "urn:uuid:dbb4f26a-b2a8-4726-8822-d40a08e67328";                      //Observation/SleepDuration
            bundleEntry13.Resource = ResourcePopulator.populateSleepDurationResource();
            WellnessRecordBundle.Entry.Add(bundleEntry13);

            var bundleEntry14 = new Bundle.EntryComponent();
            bundleEntry14.FullUrl = "urn:uuid:93ed4e3c-bfd8-4336-aa84-4cea378c655a";                                  //Observation/BodyFatMass
            bundleEntry14.Resource = ResourcePopulator.populateBodyFatMassResource();
            WellnessRecordBundle.Entry.Add(bundleEntry14);

            var bundleEntry15 = new Bundle.EntryComponent();
            bundleEntry15.FullUrl = "urn:uuid:d6878a47-b725-4f91-b90c-1e24f22f09c0";                          //Observation/BloodGlucose
            bundleEntry15.Resource = ResourcePopulator.populateBloodGlucoseResource();
            WellnessRecordBundle.Entry.Add(bundleEntry15);

            var bundleEntry16 = new Bundle.EntryComponent();
            bundleEntry16.FullUrl = "urn:uuid:72776d5e-1a2b-4351-9901-4a7ae5707500";                                         //Observation/FluidIntake
            bundleEntry16.Resource = ResourcePopulator.populateFluidIntakeResource();
            WellnessRecordBundle.Entry.Add(bundleEntry16);

            var bundleEntry17 = new Bundle.EntryComponent();
            bundleEntry17.FullUrl = "urn:uuid:3cf120f5-bb5f-411c-98fd-1d1d52d399f8";                                                 //Observation/CalorieIntake
            bundleEntry17.Resource = ResourcePopulator.populateCalorieIntakeResource();
            WellnessRecordBundle.Entry.Add(bundleEntry17);

            var bundleEntry18 = new Bundle.EntryComponent();
            bundleEntry18.FullUrl = "urn:uuid:527524b3-14e1-4a56-a459-e0af928b85bb";                                      //Observation/AgeOfMenarche
            bundleEntry18.Resource = ResourcePopulator.populateAgeOfMenarcheResource();
            WellnessRecordBundle.Entry.Add(bundleEntry18);

            var bundleEntry19 = new Bundle.EntryComponent();
            bundleEntry19.FullUrl = "urn:uuid:ebde97f8-dd36-4fd2-b8e5-6e3ff12b61dd";          //Observation/LastMenstrualPeriod
            bundleEntry19.Resource = ResourcePopulator.populateLastMenstrualPeriodResource();
            WellnessRecordBundle.Entry.Add(bundleEntry19);


            var bundleEntry20 = new Bundle.EntryComponent();
            bundleEntry20.FullUrl = "urn:uuid:e4d2d422-6d86-4e93-9c3d-80190ed709f9";                                   //Observation/DietType
            bundleEntry20.Resource = ResourcePopulator.populateDietTypeResource();
            WellnessRecordBundle.Entry.Add(bundleEntry20);

            var bundleEntry21 = new Bundle.EntryComponent();
            bundleEntry21.FullUrl = "urn:uuid:f11dd16c-37a7-4ded-9288-9f1b806d4911";                                             //Observation/TobaccoSmokingStatus
            bundleEntry21.Resource = ResourcePopulator.populateTobaccoSmokingStatusResource();
            WellnessRecordBundle.Entry.Add(bundleEntry21);
            
            var bundleEntry22 = new Bundle.EntryComponent();
            bundleEntry22.FullUrl = "urn:uuid:a9a3d290-a2c5-4b0c-8d3a-977625273136";                                 //Observation/oxygen-saturation
            bundleEntry22.Resource = ResourcePopulator.populateOxygenSaturationResource();
            WellnessRecordBundle.Entry.Add(bundleEntry22);
            
            var bundleEntry23 = new Bundle.EntryComponent();
            bundleEntry23.FullUrl = "urn:uuid:b9768f37-82cb-471e-934f-71b9ce233656";
            bundleEntry23.Resource = ResourcePopulator.populateSecondOrganizationResource();
            WellnessRecordBundle.Entry.Add(bundleEntry23);
            
            var bundleEntry24 = new Bundle.EntryComponent();
            bundleEntry24.FullUrl = "urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24";
            bundleEntry24.Resource = ResourcePopulator.populateOrganizationResource();
            WellnessRecordBundle.Entry.Add(bundleEntry24);
            
            var bundleEntry25 = new Bundle.EntryComponent();
            bundleEntry25.FullUrl = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";
            bundleEntry25.Resource = ResourcePopulator.populateDocumentReferenceResource();
            WellnessRecordBundle.Entry.Add(bundleEntry25);

            return WellnessRecordBundle;
        }
    }
}
