//------------------------------------------------------------------------------
// <auto-generated>
//     Denna kod har genererats av ett verktyg.
//     Körtidsversion:4.0.30319.18444
//
//     Ändringar i denna fil kan orsaka fel och kommer att förloras om
//     koden återgenereras.
// </auto-generated>
//------------------------------------------------------------------------------
using System;

//All classes that want to execute methods in a thread using RunThreadableJob must implement this interface.
public interface ThreadableI
{
	void preCalc();
	void afterCalc();
}