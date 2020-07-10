# PSAsync

[![.NET Core](https://github.com/aetos382/PSAsync/workflows/.NET%20Core/badge.svg?branch=master)](https://github.com/aetos382/PSAsync/actions?query=workflow%3A%22.NET+Core%22)

## 諸注意

本ライブラリは趣味の産物であり、永遠のアルファ版です。破壊的変更もガンガンしますのでご留意ください。

.NET Core 3.1 以降専用（PowerShell 7 以降専用）です。

NuGet パッケージは（まだ）ありません。

## 概要

PowerShell モジュールを .NET で書く際に、async / await を自然に使えるようにするためのライブラリです。

通常、PowerShell モジュールを .NET で書く際には、[Cmdlet](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.cmdlet?view=powershellsdk-7.0.0) クラスを継承し、[ProcessRecord](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.cmdlet.processrecord?view=powershellsdk-7.0.0) 等のメソッドをオーバーライドしますが、これらのメソッドは async ではないため、その中で非同期メソッドを自然に呼び出すことができません。

また、コマンドから結果を出力するための [WriteObject](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.cmdlet.writeobject?view=powershellsdk-7.0.0) 等のメソッドにはスレッド依存性があり、メイン スレッド以外から呼び出すと例外が発生してしまいます。

```cs
[Cmdlet("Test", "Command")]
public class TestCommand :
    Cmdlet
{
    protected override void ProcessRecord()
    {
        this.WriteObject("Hello, World.");
    }
}
```

そこで、こうした非同期メソッドを違和感なく使えるようにするためのライブラリが本品です。

```cs
[Cmdlet("Test", "AsyncCommand")]
public class TestAsyncCommand :
    Cmdlet,
    IAsyncCmdlet
{
    public async Task ProcessRecordAsync(
        CancellationToken cancellationToken)
    {
        // 非同期メソッドが使える
        await Task.Delay(100);

        // ワーカースレッドからでも大丈夫
        await Task.Run(async () => {
            await this.WriteObjectAsync("Hello, Async World.");
        });
    }

    // これを書く必要があります
    protected override void ProcessRecord()
    {
        this.DoProcessRecordAsync();
    }
}
```

## 特徴

コマンド クラスは IAsyncCmdlet インターフェイスを実装していればよく、AsyncCmdlet のような基底クラスから派生することを要求しません（一応 AsyncCmdlet / AsyncPSCmdlet は用意していますが、使う必要はありません）。

そのため、クラス設計の自由度が高くなっています。
