this example send and read 8 byte to the K64 generic HID Example
the PDF tell us to work with an example from code project

i tried it and it did not worked for me , so i created a LibUsbDotNet Small
App

to run the test i used FRDM K64 and the example usb_device_hid_generic

also , to run the USB example we need to run the install-filter-win.exe
it will open a list of devices , select the device that k64 expose
(0x15A2,0x007F)

 UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x15A2, 0x007F);
 
 once you do that , the libusb will find the device, if you dont run the
 wizard it will not find the device.
 
 there are 2 endpoints in the example of the k64 
 endp2 is write (8 byte)
 and endp1 (0x81) is the read
 