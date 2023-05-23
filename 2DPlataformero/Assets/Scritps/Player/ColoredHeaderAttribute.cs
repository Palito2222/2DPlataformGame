using System;

internal class ColoredHeaderAttribute : Attribute
{
    private string v;

    public ColoredHeaderAttribute(string v)
    {
        this.v = v;
    }
}