using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using FilesChecker;

namespace SPT.Reflection.Utils;

public static class PatchConstants
{
    public static BindingFlags PrivateFlags { get; private set; }
    public static BindingFlags PublicFlags { get; private set; }
    public static BindingFlags PublicDeclaredFlags { get; private set; }
    public static Type[] EftTypes { get; private set; }
    public static Type[] FilesCheckerTypes { get; private set; }
    public static Type LocalGameType { get; private set; }
    public static Type ExfilPointManagerType { get; private set; }
    public static Type SessionInterfaceType { get; private set; }
    public static Type BackendSessionInterfaceType { get; private set; }
    public static Type BackendProfileInterfaceType { get; private set; }

    private static ISession _backEndSession;
    public static ISession BackEndSession
    {
        get
        {
            if (_backEndSession == null)
            {
                _backEndSession = Singleton<
                    ClientApplication<ISession>
                >.Instance.GetClientBackEndSession();
            }

            return _backEndSession;
        }
    }

    static PatchConstants()
    {
        _ = nameof(ISession.GetPhpSessionId);

        PrivateFlags =
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        PublicFlags = BindingFlags.Public | BindingFlags.Instance;
        PublicDeclaredFlags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        EftTypes = typeof(AbstractGame).Assembly.GetTypes();
        FilesCheckerTypes = typeof(ICheckResult).Assembly.GetTypes();
        LocalGameType = EftTypes.SingleCustom(x => x.Name == "LocalGame");
        ExfilPointManagerType = EftTypes.SingleCustom(x =>
            x.GetMethod("InitAllExfiltrationPoints") != null
        );
        SessionInterfaceType = EftTypes.SingleCustom(x =>
            x.GetMethods().Select(y => y.Name).Contains("GetPhpSessionId") && x.IsInterface
        );
        BackendSessionInterfaceType = EftTypes.SingleCustom(x =>
            x.GetMethods().Select(y => y.Name).Contains("ChangeProfileStatus") && x.IsInterface
        );
        BackendProfileInterfaceType = EftTypes.SingleCustom(x =>
            x.GetMethods().Length == 2
            && x.GetMethods().Select(y => y.Name).Contains("get_Profile")
            && x.IsInterface
        );
    }

    /// <summary>
    /// A custom LINQ .Single() implementation with improved logging for easier patch debugging
    /// </summary>
    /// <returns>A single member of the input sequence that matches the given search pattern</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static T SingleCustom<T>(this IEnumerable<T> types, Func<T, bool> predicate)
        where T : MemberInfo
    {
        if (types == null)
        {
            throw new ArgumentNullException(nameof(types));
        }

        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        var matchingTypes = types.Where(predicate).ToArray();

        if (matchingTypes.Length > 1)
        {
            throw new InvalidOperationException(
                $"More than one member matches the specified search pattern: {string.Join(", ", matchingTypes.Select(t => t.Name))}"
            );
        }

        if (matchingTypes.Length == 0)
        {
            throw new InvalidOperationException(
                "No members match the specified search pattern"
            );
        }

        return matchingTypes[0];
    }
}