# Essentials

##Enclosed Tuple

###Usage

sb = New StringBuilder().AppendToEnclosedTuple("Foo").AppendToEnclosedTuple("Bar").AppendToEnclosedTuple("Mambo#5")

"#Foo#Bar#Mambo\#\5"

Dim tupleElements As String() = "#Foo#Bar#Mambo\#\5".SplitEnclosedTuple()

https://github.com/SmartStandards/Essentials

[![Build status](https://dev.azure.com/SmartOpenSource/Smart%20Standards%20(Allgemein)/_apis/build/status/SmartStandards.Essentials)](https://dev.azure.com/SmartOpenSource/Smart%20Standards%20(Allgemein)/_build/latest?definitionId=15)