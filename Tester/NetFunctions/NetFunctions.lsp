;; (vlisp-compile 'st "C:/Users/Joshua.Overkamp/Documents/Autodesk Plugins/PluginExample/Source/NetFunctions/NetFunctions.lsp")
(vl-load-com)
(vl-acad-defun 'ThrowLispError)
(defun net-invoke (xFunction args / sym Handler Message Result)
  (setq sym (vl-symbol-value xFunction))
	(and
		(not
			(or
				(equal (type sym) 'SUBR)
				(equal (type sym) 'USUBR)
				(equal (type sym) 'EXRXSUBR)
			)
		)
		(ThrowLispError 
			(strcat "no function definition: " (vl-symbol-name xFunction))
		)
	)
	(setq Handler *error*)
	(defun *error* (msg /)
		(setq Message msg)
	)
	(setq Result (apply xFunction args))
	(setq *error* Handler)
	(and
		Message
		(ThrowLispError Message)
	)
	Result
)
(defun NetLispTest (args)
	(net-invoke 'ADN-f894e51b-3905-4fe0-aef1-743aa83db0b9 args)
)
(princ)