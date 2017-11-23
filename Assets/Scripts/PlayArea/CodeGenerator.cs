public class CodeGenerator
{
    private static CodeGenerator instance;
    private static  object syncRoot = new object();

    public const int RequiredPhoneNumberDigits = 4;
    public const int RequiredDoorCodeDigits = 4;

    private CodeGenerator() {
		var rnd = new System.Random();
	    PhoneNumber = new int[RequiredPhoneNumberDigits];
	    for (var i = 0; i < RequiredPhoneNumberDigits; i++)
	    {
	        PhoneNumber[i] = rnd.Next(10);
	    }

        DoorCode = new int[RequiredDoorCodeDigits];
	    for (var i = 0; i < RequiredDoorCodeDigits; i++)
	    {
	        DoorCode[i] = rnd.Next(10);
	    }
    }

    public static CodeGenerator Instance
    {
        get
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new CodeGenerator();
                }
            }
            return instance;
        }
    }

    public int[] PhoneNumber { get; private set; }

    public int[] DoorCode { get; private set; }
}
