﻿using System;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;
using GlobalPayments.Api.Services;
using GlobalPayments.Api.Utils.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GlobalPayments.Api.Tests.GpApi.GpApi3DSTestCards;

namespace GlobalPayments.Api.Tests.GpApi {
    [TestClass]
    public class GpApi3DSecure1Tests : BaseGpApiTests {
        #region Constants

        private const string CHALLENGE_REQUIRED = "CHALLENGE_REQUIRED";
        private const string ENROLLED = "ENROLLED";
        private const string NOT_ENROLLED = "NOT_ENROLLED";

        #endregion

        private static CreditCardData card;
        private const string Currency = "GBP";
        private static readonly decimal Amount = new decimal(10.01);

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            ServicesContainer.ConfigureService(new GpApiConfig {
                AppId = AppId,
                AppKey = AppKey,
                Country = "GB",
                ChallengeNotificationUrl = "https://ensi808o85za.x.pipedream.net/",
                MethodNotificationUrl = "https://ensi808o85za.x.pipedream.net/",
                MerchantContactUrl = "https://enp4qhvjseljg.x.pipedream.net/",
                // RequestLogger = new RequestFileLogger(@"C:\temp\gpapi\requestlog.txt"),
                RequestLogger = new RequestConsoleLogger(),
                EnableLogging = true
            });
        }       

        public GpApi3DSecure1Tests() {
            card = new CreditCardData {
                Number = CARDHOLDER_ENROLLED_V1,
                ExpMonth = ExpMonth,
                ExpYear = ExpYear,
                CardHolderName = "John Smith"
            };
        }

        [TestMethod]
        public void CheckEnrollment_V1() {           
            var exceptionCaught = false;
            try {
                Secure3dService.CheckEnrollment(card)
                    .WithCurrency(Currency)
                    .WithAmount(Amount)
                    .Execute(Secure3dVersion.One);
            }
            catch (BuilderException ex) {
                exceptionCaught = true;
                Assert.AreEqual("3D Secure One is no longer supported!", ex.Message);
            }
            finally {
                Assert.IsTrue(exceptionCaught);
            }
        }

        [TestMethod]
        public void CardHolderEnrolled_v1() {
            var secureEcom = Secure3dService.CheckEnrollment(card)
                .WithCurrency(Currency)
                .WithAmount(Amount)
                .Execute();

            AssertThreeDSResponse(secureEcom, CHALLENGE_REQUIRED, ENROLLED);
            Assert.IsTrue(secureEcom.ChallengeMandated);
        }

        [TestMethod]
        public void CardHolderNotEnrolled_v1() {
            card.Number = CARDHOLDER_NOT_ENROLLED_V1;

            var secureEcom = Secure3dService.CheckEnrollment(card)
                .WithCurrency(Currency)
                .WithAmount(Amount)
                .Execute();

            AssertThreeDSResponse(secureEcom, NOT_ENROLLED, NOT_ENROLLED);
        }

        [TestMethod]
        public void GetAuthenticationData_V1() {
            var exceptionCaught = false;
            try {
                Secure3dService.GetAuthenticationData()
                    .Execute(Secure3dVersion.One);
            }
            catch (BuilderException ex) {
                exceptionCaught = true;
                Assert.AreEqual("3D Secure One is no longer supported!", ex.Message);
            }
            finally {
                Assert.IsTrue(exceptionCaught);
            }
        }

        private void AssertThreeDSResponse(ThreeDSecure secureEcom, string status, string enrolled) {
            Assert.IsNotNull(secureEcom);
            Assert.AreEqual(enrolled, secureEcom.Enrolled, "Card not enrolled");
            Assert.AreEqual(Secure3dVersion.One, secureEcom.Version);
            Assert.AreEqual(status, secureEcom.Status);
            Assert.IsNotNull(secureEcom.IssuerAcsUrl);
            Assert.IsNotNull(secureEcom.PayerAuthenticationRequest);
            Assert.IsNotNull(secureEcom.ChallengeReturnUrl);
            Assert.IsNotNull(secureEcom.MessageType);
            Assert.IsNotNull(secureEcom.SessionDataFieldName);
            Assert.IsNull(secureEcom.Eci);
            Assert.AreEqual("NO",secureEcom.LiabilityShift);
        }

    }
}