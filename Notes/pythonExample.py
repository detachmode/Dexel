def main():
    inputs = ReadNumbersFromCmd()
    inputs = FindAnswer(inputs)
    PrintNumbers(inputs)



def ReadNumbersFromCmd():
    while True:
        try:
            inputStr = raw_input()
            yield int(inputStr)
        except ValueError:
            print("Not a number!")



def FindAnswer(inputs):
    for number in inputs:
        if number == 42:
            raise StopIteration
        yield number


def PrintNumbers(inputs):
    for number in inputs:
        print number


main()
