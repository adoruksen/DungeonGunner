using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
    /// <summary>
    /// Empty string debug check
    /// </summary>
   public static bool ValidateCheckEmptyString(Object thisObject,string fieldName,string stringToCheck)
    {
        if (stringToCheck=="")
        {
            Debug.Log($"{fieldName} is empty and must contain a value in object {thisObject.name}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// null value debug check
    /// </summary>
    public static bool ValidateCheckNullValue(Object thisObject,string fieldNme , UnityEngine.Object objectToCheck)
    {
        if(objectToCheck == null)
        {
            Debug.Log($"{fieldNme} is null and must contain a value in object {thisObject.name}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// list empty or contains null value check - returns true if there is no error
    /// </summary>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName , IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        if (enumerableObjectToCheck ==null)
        {
            Debug.Log($"{fieldName} is null in object {thisObject.name}");
            return true;
        }
        foreach (var item in enumerableObjectToCheck)
        {
            if (item==null)
            {
                Debug.Log($"{fieldName} has null values in object {thisObject.name}");
                error = true;
            }
            else
            {
                count++;
            }
        }
        if (count==0)
        {
            Debug.Log($"{fieldName} has no values in object {thisObject.name}");
            error = true;
        }
        return error;
    }

    /// <summary>
    /// positive value debug check, if zero is allowed set iszeroallowed true. returns true if there is an error
    /// </summary>
    public static bool ValidateChechPositiveValue(Object thisObject,string fieldName,int valueToCheck,bool isZeroAllowed)
    {
        var error = false;
        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log($"{fieldName} must contain a positive value or zero in object {thisObject.name}");
                error = true;
            }
        }
        else
        {
            if (valueToCheck <= 0)
            {
                Debug.Log($"{fieldName} must contain a positive value or zero in object {thisObject.name}");
                error = true;
            }
        }
        return error;
    }
}
