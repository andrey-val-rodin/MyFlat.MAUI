using MyFlat.Maui.Common;
using MyFlat.Maui.Dto;

namespace Tests.Common
{
    public class MeterChildDtoExtensionsTests
    {
        [Fact]
        public void GetDate_CorrectValue_ValidResult()
        {
            var expected = new DateTime(2021, 7, 22, 21, 13, 6);
            var actual = new MeterChildDto
            { Dt_last_indication = "2021-07-22 21:13:06.0" }.GetDate();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetDate_InvalidValue_Null()
        {
            Assert.Null(new MeterChildDto { Dt_last_indication = "2021-07-22 21:13" }.GetDate());
        }
    }
}
