Blogpost: https://redteamer.tips/introducing-gg-aesy-a-stegocryptor/

**WARNING: you might need to restore NuGet packages before compiling. If anyone knows how I can get rid of this problem, DM me.**

<!-- wp:heading {"align":"center"} -->
<h2 class="has-text-align-center">Manual</h2>
<!-- /wp:heading -->

<!-- wp:paragraph -->
<p><strong>To start off,</strong> I highly recommend to always use GG-AESY using verbose mode or very verbose mode, if you are not using this in unmanaged loaders, I also recommend always specifying an outfile.</p>
<!-- /wp:paragraph -->

<!-- wp:paragraph -->
<p> pay attention with very verbose mode though, especially if you are hiding big payloads. as very verbose mode will print the byte array to console. </p>
<!-- /wp:paragraph -->

<!-- wp:paragraph -->
<p>having said that, let's dive into the manual for this baby. </p>
<!-- /wp:paragraph -->

<!-- wp:code -->
<pre class="wp-block-code"><code>  _______   _______                    ___       _______     _______.____    ____
 /  _____| /  _____|                  /   \     |   ____|   /       |\   \  /   /
|  |  __  |  |  __      ______       /  ^  \    |  |__     |   (----` \   \/   /
|  | |_ | |  | |_ |    |______|     /  /_\  \   |   __|     \   \      \_    _/
|  |__| | |  |__| |                /  _____  \  |  |____.----)   |       |  |
 \______|  \______|               /__/     \__\ |_______|_______/        |__|


        V1.0.0 by twitter.com/Jean_Maes_1994

        Encryptor and (optional) stegano

 Usage:
  -h, -?, --help             Show Help


  -e, --encrypt-only         Only encrypts given payload

  -d, --decrypt              decryption mode

      --ps, --payload-size=VALUE
                             only needed if extracting payload from image for
                               decryption

      --ef, --encrypted-file=VALUE
                             ENCRYPTION: The outfile for encrypted data

                               DECRYPTION:The inputfile needed to decrypt the
                               payload.




  -p, --payload=VALUE        The path to the payload you want to encrypt

  -o, --outfile=VALUE        The path to the outfile where all important data
                               will be written to (key,iv and encrypted
                               payload)

  -i, --image=VALUE          The image file to hide the key and/or IV in,
                               currently only supports JPEG (JPG) format!

      --ok, --offset-key=VALUE
                             The offset to search for the key in image (in
                               decimal)

      --okh, --offset-key-hex=VALUE
                             The offset to search for the key in image (in
                               hex)

      --oIV, --offset-IV=VALUE
                             The offset to search for the IV in image (in
                               decimal)

      --oIVh, --offset-IV-hex=VALUE
                             The offset to search for the IV in image (in
                               hex)

      --op, --offset-payload=VALUE
                             The offset to search for the payload in image
                               (in decimal)

      --oph, --offset-payload-hex=VALUE
                             The offset to search for the payload in image
                               (in hex)

  -v, --verbose              write all the good stuff to console,recommended
                               you actually always use this.

      --vv, --very-verbose   prints encrypted payload array to console
  -k, --key=VALUE            in case you want to use your own key value!

      --IV, --initialization-vector=VALUE
                             in case you want to use your own IV

      --rk, --random-key-mode
                             will hide your key in a random insertion point
                               in the provided image, without breaking said
                               image. will print the offset to console

      --ra, --random-all-mode
                             will hide both Key and IV in a random insertion
                               point of the image.

      --ak, --append-key-mode
                             will hide the key at the end of the image file

      --aa, --append-all-mode
                             will hide the key and the IV at the end of the
                               image file.

      --ap, --append-payload-mode
                             will hide the payload at the end of the image
                               file

      --rp, --random-payload-mode
                             will hide the payload at a random insertion
                               point.

      --apu, --append-payload-unencrypted
                             appends your payload without crypto, useful for
                               very quick and dirty data exfil.</code></pre>
<!-- /wp:code -->

<!-- wp:paragraph -->
<p><strong>-e or --encrypt-only</strong>: Will only encrypt a given payload (-p) will write key/iv to console if using verbose mode,  will write key/iv/payload into an outfile if using the outfile (-o) flag, and finally will write the bytestream to another file if using the encrypted file (-ef) flag.</p>
<!-- /wp:paragraph -->

<!-- wp:paragraph -->
<p><strong>-d or --decrypt</strong>: Decryption mode, you can specify the decryption parameters using offsets (in case you have hidden key or key and IV in a JPEG). Offsets are passed to the program using either the offset-key (-ok) or offset-key-hex (-okh) flags, you can use "-" as separators or just paste in the hex without any separators, both will work fine.  IV's work the same way using -oIV and -oIVh flags.<br><br>Alternatively, you can give the IV and Key directly (in case they are not hidden in a JPEG), using the key (-k) and initialization-vectors  (-IV) flags. As with the offset flags, "-" can be used as a separator, GG-AESY accepts both ASCII and byte values. <br><br>In order to decrypt, you'll also need to specify an encrypted file (-ef).  <br>Should you have hidden a payload in a JPEG and wish to decrypt it, you'll have to specify the payload size (-ps) so GG-AESY will extract all data correctly without false positives/false negatives :) . </p>
<!-- /wp:paragraph -->

<!-- wp:paragraph -->
<p><strong>-u or --unpack</strong>: Will unpack unencrypted appended payloads (=apu mode) from the JPEG. </p>
<!-- /wp:paragraph -->

<!-- wp:spacer -->
<div style="height:100px" aria-hidden="true" class="wp-block-spacer"></div>
<!-- /wp:spacer -->

<!-- wp:heading -->
<h2><strong>Stego modes:</strong></h2>
<!-- /wp:heading -->

<!-- wp:paragraph -->
<p>If no key/iv is provided, random key/iv's will be used to encrypt your data. All stego modes will require you to pass GG-AESY a JPEG image (-i). If you have specified an outfile (-o) to save your important information about the crypto ( such as key, iv, payload), all stego modes will also add the injection places in this file.</p>
<!-- /wp:paragraph -->

<!-- wp:paragraph -->
<p><strong>-rk or --random-key-mode: </strong>This Stego mode will hide your AES-256 key at a random injection point. </p>
<!-- /wp:paragraph -->

<!-- wp:paragraph -->
<p><strong>-ra or --random-all-mode: </strong>This Stego mode will hide both your AES-256 key and IV at a random injection point, both injection points <strong>can</strong> be the same (it's a random selection process), in this case, the key and IV will be injected back to back. </p>
<!-- /wp:paragraph -->

<!-- wp:paragraph -->
<p><strong>-ak or --append-key-mode</strong>: This Stego mode will append the AES-256 key at the end of the JPEG. </p>
<!-- /wp:paragraph -->

<!-- wp:paragraph -->
<p><strong>-aa or --append-all-mode:</strong> This Stego mode will append both AES-256 key and IV at the end of the JPEG. </p>
<!-- /wp:paragraph -->

<!-- wp:paragraph -->
<p><strong>-ap or --append-payload-mode: </strong>This Stego mode will append the encrypted payload bytestream to the end of the JPEG. </p>
<!-- /wp:paragraph -->

<!-- wp:paragraph -->
<p><strong>-rp or --random-payload-mode:</strong> This Stego mode will inject the encrypted payload bytestream at a random injection point. <strong>CAUTION:</strong> This only works if your payload does not exceed 65,535 bytes, which is about 65kb, if you try a larger payload, an error will be thrown in your face.  Needless to say, this mode is practically useless :) </p>
<!-- /wp:paragraph -->

<!-- wp:paragraph -->
<p><strong>-apu or --append-payload-unencrypted</strong>: This Stego mode will append the payload bytestream as-is to the end of the JPEG. </p>
<!-- /wp:paragraph -->

<!-- wp:spacer -->
<div style="height:100px" aria-hidden="true" class="wp-block-spacer"></div>
<!-- /wp:spacer -->

<!-- wp:paragraph -->
<p><strong>DISCLAIMER:</strong> This tool is in <strong>EARLY BETA</strong>. It's not been battle tested yet, so please submit improvements through PR's or raise issues in case of bugs. However, due to my current workload, active development on this tool from my end will not be possible at this time. <br>This does not mean I'm abandoning this project though :) </p>
<!-- /wp:paragraph -->
