public class BoyToilet : Toilet
{
  public override bool GenderIsFit(Unit character) => character is Boy;
}