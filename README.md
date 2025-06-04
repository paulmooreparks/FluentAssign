# FluentAssign

**FluentAssign** is a lightweight .NET library that provides a fluent, composable API for conditional assignment and type-safe conversion. It allows developers to streamline logic where values must be sourced from one or more fallbacks, transformed if necessary, and defaulted gracefully.

> Created and maintained by Paul M. Parks.

---

## ✨ Features

- ✅ Fluent, chainable assignment syntax
- 🔁 Multiple fallback candidates
- 🔄 Type conversion with inline or shared converters
- ⚙️ Deferred evaluation via lambda functions
- 🛠 Optional default fallback values or factories
- 📎 Compact and expressive alternative to nested `if` or `switch` statements

---

## 🧱 Installation

You can add FluentAssign to your project via a manual file import or future NuGet package.

```bash
dotnet add package FluentAssign
```

_**Note:** The package is not yet published. Until then, clone and reference locally._

---

## 🚀 Quick Start

```csharp
using ParksComputing.FluentAssign;

long mileage = Assign<long>
    .Create()
    .Convert<string>(x => (long)Math.Floor(Convert.ToDecimal(x)))
    .Convert<long>(x => x)
    .AssignIf(item.Release2YearMileage)
    .AssignIf(item.ReleaseMileage)
    .Else("0")
    .To();
```

---

## 🧩 Usage Scenarios

### 1. Convert and Fallback

```csharp
DateTime date = Assign<DateTime>
    .Create()
    .Convert<string>(s => DateTime.Parse(s))
    .AssignIf(config.PrimaryDate)
    .Else(config.FallbackDate)
    .Default(DateTime.MinValue)
    .To();
```

### 2. Predicate-based filtering

```csharp
string result = Assign<string>
    .Create()
    .AssignIf(() => maybeValue, v => !string.IsNullOrWhiteSpace(v))
    .Else("default")
    .To();
```

### 3. Deferred fallback using factory

```csharp
int count = Assign<int>
    .Create()
    .AssignIf(config.Count)
    .Default(() => expensiveFallbackComputation())
    .To();
```

---

## 🔍 API Overview

### Static Entrypoint

```csharp
Assign<TOut>.Create();               // Start chain
Assign<TOut>.AssignIf<T>(value);    // Start with a value
```

### Chain Methods

- `.AssignIf(value)`
- `.AssignIf(value, predicate)`
- `.AssignIf(value, predicate, converter)`
- `.Else(value)`
- `.Convert<TIn>(converter)`
- `.Default(value)`
- `.Default(factory)`
- `.To()` → returns `TOut`
- `.ToNullable()` → returns `TOut?` (if `TOut` is a value type)

---

## ✅ Requirements

- .NET 6.0 or later
- C# 10 or later recommended

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## 🤝 Contributions

Contributions are welcome! Please fork the repository and submit a pull request. Bug reports and suggestions via Issues are appreciated as well.

---

## 📬 Contact

For issues, feedback, or consulting inquiries, contact:

**Paul M. Parks**  
[parkscomputing.com](https://parkscomputing.com)
