**Addendum IPPCheck**

This update was designed to address a few issues I found while testing the IPP code contained in the book.  I did not want to remove the original chapter code contents - thus readers can still track the initial code with the book contents. Instead, this update provides a few fixes I though were necessary to further increase the reliability of operation. For this addendum, the changes were made to the underlying CsIppRequestLib DLL which are compiled into IPPCheck - therefore it tracks with the description in the book.

**The ReadAsync issue:** This issue primarily affects the way the returned byte stream from the printer tracks with ResponseHelper class. The problem can arise due to the way the network stack in the OS (Windows in this case) sends byte streams to user mode applications waiting for a response. The OS guarantees ordered sequence of bytes to the user-space, but absolutely no guarantees are inferred about how many bytes are delivered per read. And therein lies the problem: ReadAsync guarantees up to the number requested, and it may return 0 only when the stream has reached end-of-stream, but *no guarantee is made that the number of bytes you request are available*. The salient point is read sizes are an application concern, not guaranteed by TCP.  Thus, if you request ReadAsync as below it is allowed to complete as soon as any bytes are available (in this case up to 11). It does not promise to fill that buffer however, and here is where the first sign of trouble might arise. IPP is length-prefixed and tag/field oriented; if you’re off by even a single byte, subsequent size reads are invalid and can destroy integrity and create parsing errors.

// Asynchronously read 11 bytes into the byte array

` `await stream.ReadAsync(dtArray, 0, 11);

Thus, this call Asynchronously reads a sequence of bytes from the current stream, and advances the position within the stream by the number of bytes it read. However, if you assume on your end that it has read 11 bytes (and it only has read 5 or 6), this will throw off the position within any structure (like within an IPP HTTP packet structure) that you are following. This is felt specifically for IPP byte sequences, as they maintain a relative positional meaning within the returned stream. One solution for this might be to loop through ReadAsync calls until the requested number of bytes from the caller is achieved, as shown below:

while (total < count)

{

` 	`int n = await s.ReadAsync(buf, offset + total, count - total, ct).ConfigureAwait(false);

`  	`if (n == 0)

`          `{

`          	`throw new Exception($"Unexpected EOF. Needed {count} bytes, got {total}.");

`          `}

`          `total += n;

`   `}



Accordingly, this is what the update does.  The ResponseHelpers class is essentially identical to the previous version except the individual Stream.ReadAsync calls have been replaced with SafeStreamRead calls that support cancellation tokens and exact reads in one call – thus avoiding the cause of various bugs when reading IPP responses. Note: All methods have been updated to replace stream.ReadAsync with the (safer) SafeStreamRead methods. You might be wondering why ReadAsync behaves this way: it is by design - streams are optimized for continual consumption (e.g., media streaming). Returning available bytes avoids unnecessary waiting to “fill x bytes”, reduces latency, and allows seamless transfer of streaming data.

**Modification of the CompletionStruct object**: This was necessitated when creating print jobs with a password assigned – these become stored on the printer in memory until the user walks up to the printer and supplies the necessary PIN or password. The former CompletionStruct did not include job state, job state text, or job state reason(s). This becomes particularly acute when the print job is sent to the printer via Send-Document. In my testing, the job status typically returned 0 (success), but in the event the printer could not properly render the job the job storage step would be cancelled. Thus, the user might be led to believe the job was ready to be pulled from the printer when in fact it was never stored at all. The new ResponseHelper class will attempt to supply further information to the user for the current state of the requested job (i.e. the stored job). On an aside, several printer manufacturers had issues with the cancellation of stored jobs when they could not properly process them. Some in fact locked up after a complex pdf was sent (exhausted memory?) which required a printer reboot to fix.

**Better handling of IPP collections:** This was probably the most difficult issue to upgrade as IPP collections are not trivial to process. The upgrade puts the recovered collections from the printer into the human readable format (discussed in Addendum 3) that displays a collection string with curly braces around the outer and subsequent nested collections. This human readable format is also shown in the Wireshark IPP dissector when collection bytes are analyzed. The main work for this is done by the ResponseHelper’s ReadCollectionBodyStringAsync method, a recursive solution to the complexity problem of IPP collections. The main work for rendering IPP collections is performed by the ReadCollectionBodyStringAsync method, which employs this approach to manage the nested structure of IPP collections. Recursion starts when a member value is tagged with BegCollection (0x38). From that point, the nested call(s) continue consuming bytes from the stream until it encounters EndCollection (0x37), which serves as the recursion base condition. When EndCollection is reached, the nested call completes and creates the string for that (nested) collection. This string is stored in the variable **nested** and appended to the caller’s StringBuilder variable **content**, allowing each recursive call to create its own nested collection string, adding that to the total (content) collection StringBuilder. This structure mirrors IPP specifications, where BegCollection and EndCollection form a properly nested delimiter pair and each recursive call consumes exactly one such delimited region from the byte stream.

Note: To debug the recursive calls, I had to open Debug Options in Visual Studio for the project and enable “Suppress JIT optimization on module load (Managed only)”.
