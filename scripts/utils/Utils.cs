using Godot;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;


public static class Utils
{
    public static int GCD(int a, int b) { return b == 0 ? a : GCD(b, a % b); }
    public struct Fraction
    {
        private int numerator = 0;
        private int denominator = 0;
        public int Numerator
        {
            get
            { return numerator; }
        }
        public int Denominator
        {
            get
            { return denominator; }
        }
        public Fraction(int numerator, int denominator)
        {
            if (denominator == 0) throw new Exception("Denominator Equals to Zero!");
            int gcd=GCD(numerator, denominator);
            this.numerator = numerator/gcd;
            this.denominator = denominator/gcd;
            if (this.denominator < 0)
            {
                this.numerator=-this.numerator;
                this.denominator=-this.denominator;
            }//Keep denominator positive.
        }
        public static Fraction operator +(Fraction a, Fraction b)
        {
            return new Fraction
                (
                a.Numerator*b.Denominator+b.Numerator*a.Denominator,
                a.Denominator*b.Denominator
                );
        }
        public static Fraction operator -(Fraction a, Fraction b)
        {
            return new Fraction
                (
                a.Numerator * b.Denominator - b.Numerator * a.Denominator,
                a.Denominator * b.Denominator
                );
        }
        public static Fraction operator +(Fraction a, int b)
        {
            return new Fraction
                (
                a.Numerator +b*a.denominator,
                a.Denominator 
                );
        }
        public static Fraction operator -(Fraction a, int b)
        {
            return new Fraction
                (
                a.Numerator - b * a.denominator,
                a.Denominator
                );
        }
        public static bool operator ==(Fraction a, Fraction b)
        {
            return (a.Reduced().Numerator == b.Reduced().Numerator)&&
                (a.Reduced().Denominator==b.Reduced().Denominator);
        }
        public static bool operator !=(Fraction a, Fraction b)
        {
            return (a.Reduced().Numerator != b.Reduced().Numerator) ||
                (a.Reduced().Denominator != b.Reduced().Denominator);
        }
        public static bool operator <(Fraction a, Fraction b)
        {
            return (a - b).Numerator < 0;
        }
        public static bool operator >(Fraction a, Fraction b)
        {
            return (a - b).Numerator > 0;
        }
        public static bool operator >=(Fraction a, Fraction b)
        {
            return (a - b).Numerator >= 0;
        }
        public static bool operator <=(Fraction a, Fraction b)
        {
            return (a - b).Numerator <= 0;
        }
        public static Fraction Zero()
        {
            return new Fraction(0, 1);
        }
        public static Fraction One()
        {
            return new Fraction(1, 1);
        }
        public int GetTrueNumerator()
        {
            return Numerator % Denominator;
        }
        public int GetWhole()
        {
            return (int)(Math.Floor((float)Numerator/Denominator));
        }

        public override string ToString()
        {
            return Numerator>Denominator?$"{Numerator}/{Denominator} = {GetWhole()}+{GetTrueNumerator()}/{Denominator}"
                :$"{Numerator}/{Denominator}";
        }
        public Fraction Reduced()
        {
            return new Fraction(this.Numerator, this.Denominator);
        }
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public static string GetPropertyName(LambdaExpression expr)
    {
        var body = expr.Body;
        //Covert value-type propertie expression to member expression
        if (body is UnaryExpression ue)
        {
            body = ue.Operand;
        }
        if (body is MemberExpression me)
        {
            return me.Member.Name;
        }
        throw new Exception("Expression is not a property expression!");
    }
}
