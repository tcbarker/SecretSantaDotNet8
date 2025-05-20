using Xunit;
using Xunit.Abstractions;
using SecretSanta.Services;
using SecretSanta.Data.Models;
using SecretSanta.Shared.Interfaces;
using SecretSanta.Shared.Models;
using SecretSanta.Shared;
using SecretSanta.Tests.Repos;

using System.Text.Json;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.DependencyInjection;

namespace SecretSanta.Tests;

public class CampaignServiceTests {
    private readonly ITestOutputHelper _output;
    private CampaignService _campaignService;
    private CampaignRepositoryFake _campaignRepository;
    private UserRepositoryFake _userRepository;
    private EmailRepositoryFake _emailRepository;

    const string myemail = "user@test";
    const string otheruseremail = "anotheruser@test";

    public CampaignServiceTests(ITestOutputHelper output) {
        _output = output;

        var serviceProvider = new ServiceCollection()
        .AddLogging(
            builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace)
        )
        .BuildServiceProvider();

        var factory = serviceProvider.GetService<ILoggerFactory>();

        _userRepository = new UserRepositoryFake();
        _campaignRepository = new CampaignRepositoryFake();
        _emailRepository = new EmailRepositoryFake();

        _campaignService = new CampaignService(
            _campaignRepository,
            _emailRepository,
            _userRepository,
            factory!.CreateLogger<CampaignService>());
    }

    void PrintCampaignDTO(CampaignDTO CampaignDTO){
        _output.WriteLine(JsonSerializer.Serialize(CampaignDTO, new JsonSerializerOptions{ ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles, WriteIndented = true }) );
    }

    public static CampaignMemberDTO? getMemberDTOFromCampaignDTO(CampaignDTO campaign, string email){
        return campaign.Members.FirstOrDefault(mem => {return mem.Email == email;});
    }



    CampaignDTO getCampaignDTO(string mainemail){
        return new CampaignDTO{
            Members = [
                new CampaignMemberDTO{
                    Email = mainemail,
                    Organiser = true,
                    DisplayEmail = null,
                    DisplayName = null,
                    Accept = null
                }
            ]
        };
    }

    async Task<Campaign> getCampaign(bool ispublic = false, string thirduseremail = "third@test.com"){
        return new Campaign{
            CreatedDate = DateTime.Now,
            Name = "Test",
            WelcomeMessage = "Welcome",
            Members = new List<CampaignMember>([
                new CampaignMember{
                    Email = await _emailRepository.getEmailAsync(myemail),
                    Accept = null,
                    Organiser = true,
                    DisplayName = "test user",
                    DisplayEmail = await _emailRepository.getEmailAsync(myemail)
                },
                new CampaignMember{
                    Email = await _emailRepository.getEmailAsync(otheruseremail),
                    Accept = null,
                    Organiser = null,
                    DisplayName = null,
                    DisplayEmail = null
                },
                new CampaignMember{
                    Email = await _emailRepository.getEmailAsync(thirduseremail),
                    Accept = true,
                    Organiser = false,
                    DisplayName = "third user",
                    DisplayEmail = await _emailRepository.getEmailAsync(thirduseremail)
                }
            ]),
            JoinPublic = ispublic,
            RequireVerification = false,
            CampaignGuid = new CampaignGuid{
                Id = new System.Guid()
            }
        };
    }

    CampaignDTO getCampaignDTOWhereNotFirstNotOrganiserAndNoDisplayEmail(string mainemail, string otheremail = "someoneelse@test.test", string othername = "somebody else"){
        return new CampaignDTO{
            Name = "test",
            WelcomeMessage = "welcome",
            Members = [
                new CampaignMemberDTO{
                    Email = otheremail,
                    Organiser = true,
                    Accept = true,
                    DisplayEmail = otheremail,
                    DisplayName = othername
                },
                new CampaignMemberDTO{
                    Email = mainemail
                }
            ]
        };
    }


    [Fact]
    public async void CreatingACampaignWithUserMissingWillFail() {
        _userRepository.FakeSetUser(["user@test"]);
        bool? success = null;
        try {
            CampaignActionDTO result = await _campaignService.CreateCampaignAsync(
                getCampaignDTO("notme@test")
            );
            success = true;
        } catch {
            success = false;
        }
        Assert.False(success);
        Assert.True(_campaignRepository.campaignindatabase==null);
    }

    [Fact]
    public async void UserWillBeMadeOrganiserWhenCreatingACampaign() {
        _userRepository.FakeSetUser([myemail]);
        CampaignActionDTO result = await _campaignService.CreateCampaignAsync(
            getCampaignDTOWhereNotFirstNotOrganiserAndNoDisplayEmail(myemail) );
        CampaignMemberDTO? returnedme = getMemberDTOFromCampaignDTO(result.Campaign, myemail);
        Assert.True(returnedme!=null);
        Assert.True(returnedme.Organiser==true);
    }

    [Fact]
    public async void OrganiserUsersDisplayEmailWillBeSetWhenCreatingACampaign() {
        _userRepository.FakeSetUser([myemail]);
        CampaignActionDTO result = await _campaignService.CreateCampaignAsync(
            getCampaignDTOWhereNotFirstNotOrganiserAndNoDisplayEmail(myemail) );
        CampaignMemberDTO? returnedme = getMemberDTOFromCampaignDTO(result.Campaign, myemail);
        Assert.True(returnedme!=null);
        Assert.True(returnedme.DisplayEmail==myemail);
    }

    [Fact]
    public async void InvalidOrganiserDisplayEmailWillBeReplacedWithMainEmailWhenCreatingACampaign() {
        const string wrongemail = "noiwontgiveit@butiamanorganiser";
        _userRepository.FakeSetUser(["another@email",myemail]);
        CampaignActionDTO result = await _campaignService.CreateCampaignAsync(
            new CampaignDTO{
                Name = "test",
                WelcomeMessage = "welcome",
                Members = [
                    new CampaignMemberDTO {
                        Email = myemail,
                        Organiser = true,
                        DisplayEmail = wrongemail,
                    }
                ]
            }
        );
        //PrintCampaignDTO(result.Campaign);
        CampaignMemberDTO? returnedme = getMemberDTOFromCampaignDTO(result.Campaign, myemail);
        Assert.True(returnedme!=null);
        Assert.True(returnedme.DisplayEmail==myemail);//not wrongemail, and not another@email, though that would be valid.
    }

    [Fact]
    public async void MemberSetAsOrganiserWillBeChangedToChanceWhenCreatingACampaign() {
        const string dontforce = "dont@forceorganiser";
        _userRepository.FakeSetUser([myemail]);
        CampaignActionDTO result = await _campaignService.CreateCampaignAsync(
            getCampaignDTOWhereNotFirstNotOrganiserAndNoDisplayEmail(myemail, dontforce) );
        CampaignMemberDTO? othermember = getMemberDTOFromCampaignDTO(result.Campaign, dontforce);
        Assert.True(othermember!=null);
        Assert.True(othermember.Organiser==null);
    }

    [Fact]
    public async void SettingOtherMembersInformationWillBeReplacedWithNullWhenCreatingACampaign() {
        _userRepository.FakeSetUser([myemail]);
        CampaignActionDTO result = await _campaignService.CreateCampaignAsync(
            getCampaignDTOWhereNotFirstNotOrganiserAndNoDisplayEmail(myemail, otheruseremail)
        );
        //PrintCampaignDTO(result.Campaign);

        CampaignMemberDTO? returnedother = getMemberDTOFromCampaignDTO(result.Campaign, otheruseremail);
        Assert.True(returnedother!=null);
        Assert.True(returnedother.DisplayName==null);
        Assert.True(returnedother.DisplayEmail==null);
        Assert.True(returnedother.Accept==null);
    }

    [Fact]
    public async void CreatedDateWillBeSetWhenCreatingACampaign() {
        _userRepository.FakeSetUser([myemail]);
        DateTime createdwith = DateTime.Now.AddYears(30);
        CampaignActionDTO result = await _campaignService.CreateCampaignAsync(
            new CampaignDTO {
                Name = "test",
                WelcomeMessage = "welcome",
                Members = [
                    new CampaignMemberDTO{
                        Email = myemail,
                        Organiser = true
                    }
                ],
                CreatedDate = createdwith
            }
        );
        Assert.False(result.Campaign.CreatedDate==createdwith);
    }

    [Fact]
    public async void CampaignGuidWillBeCreatedWhenCreatingACampaign() {
        _userRepository.FakeSetUser([myemail]);
        System.Guid Guid = new System.Guid();
        CampaignActionDTO result = await _campaignService.CreateCampaignAsync(
            new CampaignDTO {
                Name = "test",
                WelcomeMessage = "welcome",
                Members = [
                    new CampaignMemberDTO{
                        Email = myemail,
                        Organiser = true
                    }
                ],
                Guid = Guid
            }
        );
        Assert.False(result.Campaign.Guid==Guid);
    }


    [Fact]
    public async void UpdateWillFailIfCampaignNotFoundWhenUpdatingACampaign() {
        _userRepository.FakeSetUser([myemail]);
        _campaignRepository.campaignindatabase = null;
        bool? success = null;
        try {
            CampaignActionDTO result = await _campaignService.UpdateCampaignAsync(
                new System.Guid(),
                new CampaignDTO {
                    Name = "test",
                    WelcomeMessage = "welcome",
                    Members = [
                        new CampaignMemberDTO{
                            Email = myemail
                        }
                    ]
                },
                null
            );
            success = true;
        } catch {
            success = false;
        }
        Assert.False(success);
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async void UpdateWillFailIfNotLoggedInWhenUpdatingACampaign(bool ispublic) {
        _campaignRepository.campaignindatabase = await getCampaign(ispublic);
        bool? success = null;
        try {
            CampaignActionDTO result = await _campaignService.UpdateCampaignAsync(
                new System.Guid(),
                new CampaignDTO {
                    Name = "test",
                    WelcomeMessage = "welcome",
                    Members = [
                        new CampaignMemberDTO{
                            Email = myemail
                        }
                    ]
                },
                null
            );
            success = true;
        } catch {
            success = false;
        }
        Assert.False(success);
    }

    [Fact]
    public async void UpdateWillFailIfUserMissingWhenUpdatingAPrivateCampaign() {
        const string notin = "notin@test.test";
        _userRepository.FakeSetUser([notin]);
        _campaignRepository.campaignindatabase = await getCampaign();
        bool? success = null;
        try {
            CampaignActionDTO result = await _campaignService.UpdateCampaignAsync(
                new System.Guid(),
                new CampaignDTO {
                    Name = "test",
                    WelcomeMessage = "welcome",
                    Members = [
                        new CampaignMemberDTO{
                            Email = notin
                        }
                    ]
                },
                null
            );
            success = true;
        } catch {
            success = false;
        }
        Assert.False(success);
    }




    static public IEnumerable<object[]> GetUserData(bool? passednull) {
        return new List<object[]>{
            new object[] { myemail, true },
            new object[] { otheruseremail, false }
        };
    }

    [Theory]
    [MemberData( nameof(GetUserData), parameters: null )]
    public async void OnlyAnOrganiserCanChangeNameAndMessageWhenUpdatingACampaign(string email, bool isorganiser) {
        _userRepository.FakeSetUser([email]);
        _campaignRepository.campaignindatabase = await getCampaign();
        const string newname = "a new name";
        const string newwelcomemessage = "a new welcome message";

        CampaignActionDTO result = await _campaignService.UpdateCampaignAsync(
            new System.Guid(),
            new CampaignDTO {
                Name = newname,
                WelcomeMessage = newwelcomemessage,
                Members = [
                    new CampaignMemberDTO{
                        Email = email
                    }
                ]
            },
            null
        );
        //PrintCampaignDTO(result.Campaign);
        Assert.True(isorganiser==(result.Campaign.Name==newname));
        Assert.True(isorganiser==(result.Campaign.WelcomeMessage==newwelcomemessage));
    }


    [Theory]
    [MemberData( nameof(GetUserData), parameters: null )]
    public async void OnlyAnOrganiserCanAddNewMembersWhenUpdatingANonPublicCampaign(string email, bool isorganiser) {
        _userRepository.FakeSetUser([email]);
        _campaignRepository.campaignindatabase = await getCampaign(false);

        CampaignActionDTO result = await _campaignService.UpdateCampaignAsync(
            new System.Guid(),
            new CampaignDTO {
                Members = [
                    new CampaignMemberDTO{
                        Email = email
                    },
                    new CampaignMemberDTO{
                        Email = "somebodynew@test.test"
                    },
                ]
            },
            null
        );
        //PrintCampaignDTO(result.Campaign);
        Assert.True( (isorganiser?4:3)==result.Campaign.Members.Length);
    }



    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async void ANormalUserCanAddOnlyThemselfToACampaignAndOnlyIfItIsPublicAndCannotSetThemselfOrganiserWhenUpdatingACampaign(bool myself, bool ispublic){
        const string thisemail = "anewuser@test.test";
        _userRepository.FakeSetUser([thisemail]);
        _campaignRepository.campaignindatabase = await getCampaign(ispublic);

        bool? success = null;
        try {
            CampaignActionDTO result = await _campaignService.UpdateCampaignAsync(
                new System.Guid(),
                new CampaignDTO {
                    Members = [
                        new CampaignMemberDTO{
                            Email = myself?thisemail:"notmyemail@test.test",
                            Organiser = true
                        }
                    ]
                },
                null
            );
            success = true;
            CampaignMemberDTO? foundself = getMemberDTOFromCampaignDTO(result.Campaign, myself?thisemail:"notmyemail@test.test");
            Assert.True( (foundself!=null) && (foundself.Organiser==false) );
        } catch {
            success = false;
        }
        Assert.True(success == (myself&&ispublic) );
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async void OnlyAUserOfferedOrganiserStatusCanBecomeOrganiserWhenUpdatingCampaign(bool offeredorganiser, bool ispublic) {
        const string notoffered = "notoffered@organiser.thing";
        string thisemail = (offeredorganiser?otheruseremail:notoffered);
        _userRepository.FakeSetUser([thisemail]);
        _campaignRepository.campaignindatabase = await getCampaign(ispublic, notoffered);
        CampaignActionDTO result = await _campaignService.UpdateCampaignAsync(
            new System.Guid(),
            new CampaignDTO {
                Members = [
                    new CampaignMemberDTO{
                        Email = thisemail,
                        Organiser = true
                    }
                ]
            },
            null
        );
        //PrintCampaignDTO(result.Campaign);
        CampaignMemberDTO? foundself = getMemberDTOFromCampaignDTO(result.Campaign, thisemail);
        Assert.True( (foundself!=null) && (foundself.Organiser==offeredorganiser) );
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async void EmailsSeenAreLimitedToTheUsersOwnUnlessTheyAreOrganiserWhenGettingCampaign(bool isorganiser, bool ispublic){
        _userRepository.FakeSetUser([isorganiser?myemail:otheruseremail]);
        _campaignRepository.campaignindatabase = await getCampaign(ispublic);
        CampaignDTO result = await _campaignService.GetCampaignAsync(
            new System.Guid()
        );
        Assert.True(isorganiser == (result.Members[0].Email!=null) );
        Assert.True(result.Members[1].Email!=null);
        Assert.True(isorganiser == (result.Members[2].Email!=null) );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async void DisplayEmailsShouldOnlyBeGivenIfViewedByAMemberOrBelongingToOrganiserWhenGettingCampaign(bool viewedbymember){
        if(viewedbymember){
            _userRepository.FakeSetUser([otheruseremail]);
        }
        _campaignRepository.campaignindatabase = await getCampaign(true);
        CampaignDTO result = await _campaignService.GetCampaignAsync(
            new System.Guid()
        );
        Assert.True(result.Members[0].DisplayEmail!=null);
        Assert.True( viewedbymember == (result.Members[2].DisplayEmail!=null) );
        Assert.True( viewedbymember == (result.Members[2].DisplayName!=null) );
    }


    [Fact]
    public async void GetWillFailWhenCampaignNotFound() {
        _userRepository.FakeSetUser([myemail]);
        _campaignRepository.campaignindatabase = null;
        bool? success = null;
        try {
            CampaignDTO result = await _campaignService.GetCampaignAsync(
                new System.Guid()
            );
            success = true;
        } catch {
            success = false;
        }
        Assert.False(success);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async void GetWillOnlySucceedIfCampaignIsPublicWhenUserNotLoggedIn(bool ispublic) {
        _campaignRepository.campaignindatabase = await getCampaign(ispublic);
        bool? success = null;
        try {
            CampaignDTO result = await _campaignService.GetCampaignAsync(
                new System.Guid()
            );
            success = true;
        } catch {
            success = false;
        }
        Assert.True(success==ispublic);
    }




    [Fact]
    public async void GetAllCampaignsAsyncShouldReturnEmptyWhenNoUserLoggedInOrNotInACampaign() {
        Assert.Empty(await _campaignService.GetAllCampaignsAsync());
    }

    [Fact]
    public async void GetAllCampaignsAsyncShouldReturnNotEmptyWhenUserInCampaignLoggedIn() {
        _userRepository.FakeSetUser(
            [myemail],
            [await getCampaign()]
        );
        Assert.NotEmpty(await _campaignService.GetAllCampaignsAsync());
    }

}

