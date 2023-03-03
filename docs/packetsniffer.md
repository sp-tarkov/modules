# Packet Sniffer

References are based on version 0.12.12.15.17566

## Requirements

- de4dot
- dnspy

## Deobfuscation

```cs
// Token: 0x0600D716 RID: 55062 RVA: 0x00127E88 File Offset: 0x00126088
Class2082.smethod_0()
{
    return (string)((Hashtable)AppDomain.CurrentDomain.GetData(Class2082.string_0))[int_0];
}
```

```cmd
de4dot-x64.exe Assembly-CSharp.dll
de4dot-x64.exe --un-name "!^<>[a-z0-9]$&!^<>[a-z0-9]__.*$&![A-Z][A-Z]\$<>.*$&^[a-zA-Z_<{$][a-zA-Z_0-9<>{}$.`-]*$" "Assembly-CSharp-cleaned.dll" --strtyp delegate --strtok 0x0600D716
pause
```

### Fix ResolutionScope error

1. DnSpy > File > Open... > `Assembly-CSharp-cleaned-cleaned.dll`
2. DnSpy > File > Save module.. > OK

## Modifications

### Assembly-CSharp.dll

#### Save requests

```cs
// Token: 0x06001CF6 RID: 7414 RVA: 0x0019CAC8 File Offset: 0x0019ACC8
[postfix]
Class182.method_2()
{
    var uri = new Uri(url);
    var path = (System.IO.Directory.GetCurrentDirectory() + "\\HTTP_DATA\\").Replace("\\\\", "\\");
    var file = uri.LocalPath.Replace('/', '.').Remove(0, 1);
    var time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

    if (System.IO.Directory.CreateDirectory(path).Exists && obj != null)
    {
        System.IO.File.WriteAllText($@"{path}req.{file}_{time}.json", text);
    }
}
```

#### Save responses

```cs
// Token: 0x06001D01 RID: 7425 RVA: 0x0019D200 File Offset: 0x0019B400
[postfix]
Class182.method_8()
{
    // add this at the end, before "return text3;"
    // in case you turn this into a harmony patch, text3 = __result
    var uri = new Uri(url);
    var path = (System.IO.Directory.GetCurrentDirectory() + "\\HTTP_DATA\\").Replace("\\\\", "\\");
    var file = uri.LocalPath.Replace('/', '.').Remove(0, 1);
    var time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

    if (System.IO.Directory.CreateDirectory(path).Exists)
    {
        System.IO.File.WriteAllText($@"{path}resp.{file}_{time}.json", text3);
    }
}
```

#### Disable SSL certification

```cs
// Token: 0x0600509D RID: 20637 RVA: 0x0027D244 File Offset: 0x0027B444
[prefix]
Class537.ValidateCertificate()
{
    return true;
}
```

```cs
// Token: 0x0600509E RID: 20638 RVA: 0x0027D2B4 File Offset: 0x0027B4B4
[prefix]
Class537.ValidateCertificate()
{
    return true;
}
```

#### Battleye

```cs
// Token: 0x06006B7A RID: 27514 RVA: 0x002D55B8 File Offset: 0x002D37B8
[prefix]
Class815.RunValidation()
{
    this.Succeed = true;
}
```

### FilesChecker.dll

#### Consistency multi

```cs
// Token: 0x06000054 RID: 84 RVA: 0x00002A38 File Offset: 0x00000C38
[prefix]
ConsistencyController.EnsureConsistency()
{
    return Task.FromResult<ICheckResult>(ConsistencyController.CheckResult.Succeed(new TimeSpan()));
}
```

#### Consistency single

```cs
// Token: 0x06000053 RID: 83 RVA: 0x000028D4 File Offset: 0x00000AD4
[prefix]
ConsistencyController.EnsureConsistencySingle()
{
    return Task.FromResult<ICheckResult>(ConsistencyController.CheckResult.Succeed(new TimeSpan()));
}
```
