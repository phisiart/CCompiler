﻿using System;
using System.Collections.Generic;
using System.Linq;

public class Parser {
    public static Boolean IsSizeof(Token token) {
        if (token.type == TokenType.KEYWORD) {
            if (((TokenKeyword)token).val == KeywordVal.SIZEOF) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsLPAREN(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.LPAREN) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsRPAREN(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.RPAREN) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsCOLON(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.COLON) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsQuestionMark(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.QUESTION) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsAssignment(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.ASSIGN) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsCOMMA(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.COMMA) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsLCURL(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.LCURL) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsRCURL(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.RCURL) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsLBRACKET(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.LBRACKET) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsRBRACKET(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.RBRACKET) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsPERIOD(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.PERIOD) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsEllipsis(List<Token> src, Int32 begin) {
        if (Parser.IsPERIOD(src[begin])) {
            begin++;
            if (Parser.IsPERIOD(src[begin])) {
                begin++;
                if (Parser.IsPERIOD(src[begin])) {
                    return true;
                }
            }
        }
        return false;
    }

    public static Boolean IsSEMICOLON(Token token) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == OperatorVal.SEMICOLON) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsKeyword(Token token, KeywordVal val) {
        if (token.type == TokenType.KEYWORD) {
            if (((TokenKeyword)token).val == val) {
                return true;
            }
        }
        return false;
    }

    public static Boolean IsOperator(Token token, OperatorVal val) {
        if (token.type == TokenType.OPERATOR) {
            if (((TokenOperator)token).val == val) {
                return true;
            }
        }
        return false;
    }

    public static String GetIdentifierValue(Token token) {
        if (token.type == TokenType.IDENTIFIER) {
            return ((TokenIdentifier)token).val;
        } else {
            return null;
        }
    }

    public static List<Token> GetTokensFromString(String src) {
        Scanner lex = new Scanner();
        lex.src = src;
        lex.Lex();
        return lex.tokens;
    }

    // EatOperator : (src, ref current, val) -> Boolean
    // =============================================
    // tries to eat an operator
    // if succeed, current++, return true
    // if fail, current remains the same, return false
    // 
    public static Boolean EatOperator(List<Token> src, ref Int32 current, OperatorVal val) {
        if (src[current].type != TokenType.OPERATOR) {
            return false;
        }

        if (((TokenOperator)src[current]).val != val) {
            return false;
        }

        current++;
        return true;
    }

    public delegate Int32 FParse<TRet>(List<Token> src, Int32 begin, out TRet node) where TRet : PTNode;

    public static Int32 ParseOptional<TRet>(List<Token> src, Int32 begin, TRet default_val, out TRet node, FParse<TRet> Parse) where TRet : PTNode {
        Int32 current;
        if ((current = Parse(src, begin, out node)) == -1) {
            // if parsing fails: return default value
            node = default_val;
            return begin;
        } else {
            return current;
        }
    }

    public static Int32 ParseList<TRet>(List<Token> src, Int32 begin, out List<TRet> list, FParse<TRet> Parse) where TRet : PTNode {
        Int32 current = begin;

        list = new List<TRet>();
        TRet item;

        while (true) {
            Int32 saved = current;
            if ((current = Parse(src, current, out item)) == -1) {
                return saved;
            }
            list.Add(item);
        }

    }

    public static Int32 ParseNonEmptyList<TRet>(List<Token> src, Int32 begin, out List<TRet> list, FParse<TRet> Parse) where TRet : PTNode {
        begin = ParseList(src, begin, out list, Parse);
        if (list.Any()) {
            return begin;
        } else {
            return -1;
        }
    }
    
    public static Int32 Parse2Choices<TRet, T1, T2>(List<Token> src, Int32 begin, out TRet node, FParse<T1> Parse1, FParse<T2> Parse2)
        where T1 : TRet
        where T2 : TRet
        where TRet : PTNode {
        Int32 ret;

        T1 node1;
        if ((ret = Parse1(src, begin, out node1)) != -1) {
            node = node1;
            return ret;
        }

        T2 node2;
        if ((ret = Parse2(src, begin, out node2)) != -1) {
            node = node2;
            return ret;
        }
        
        node = null;
        return -1;
    }

    public static Int32 ParseNonEmptyListWithSep<TRet>(List<Token> src, Int32 pos, out List<TRet> list, FParse<TRet> Parse, OperatorVal op) where TRet : PTNode {
        list = new List<TRet>();
        TRet item;

        if ((pos = Parse(src, pos, out item)) == -1)
            return -1;
        list.Add(item);

        while (true) {
            Int32 saved = pos;
            if (!Parser.EatOperator(src, ref pos, op))
                return saved;
            if ((pos = Parse(src, pos, out item)) == -1)
                return saved;
            list.Add(item);
        }

    }
}

public class ParserEnvironment {
    public static Boolean debug = false;
}


public class Program {
    public static void Main(String[] args) {
        Scanner lex = new Scanner();
        lex.OpenFile("../../../hello.c");
        lex.Lex();
        var src = lex.tokens;
        TranslationUnit root;
        Int32 current = _translation_unit.Parse(src, 0, out root);
    }
}
