public class GirlsToilet : Toilet
{
  public override bool GenderIsFit(Unit character) => character is Girl;
}