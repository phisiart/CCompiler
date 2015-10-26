﻿using System;
using System.Collections.Generic;

namespace Parsing {

    /// <summary>
    /// A minimal environment solely for parsing, intended to resolve ambiguity.
    /// </summary>
    public sealed class ParserEnvironment {
        // TODO: design ParserEnvironment
    }

    /// <summary>
    /// The input type for every parsing function.
    /// </summary>
    public sealed class ParserInput {
        public ParserInput(ParserEnvironment environment, IEnumerable<Token> source) {
            this.Environment = environment;
            this.Source = source;
        }
        public ParserEnvironment Environment { get; }
        public IEnumerable<Token> Source { get; }
        public IParserResult<R> Parse<R>(IParser<R> parser) =>
            parser.Parse(this);
    }

    /// <summary>
    /// A parser result with/without content.
    /// </summary>
    public interface IParserResult {
        ParserInput ToInput();

        Boolean IsSuccessful { get; }

        ParserEnvironment Environment { get; }

        IEnumerable<Token> Source { get; }
    }

    /// <summary>
    /// A failed result.
    /// </summary>
    public sealed class ParserFailed : IParserResult {
        public ParserInput ToInput() {
            throw new InvalidOperationException("Parser failed, can't construct input.");
        }

        public Boolean IsSuccessful => false;

        public ParserEnvironment Environment {
            get {
                throw new NotSupportedException("Parser failed, can't get environment.");
            }
        }

        public IEnumerable<Token> Source {
            get {
                throw new NotSupportedException("Parser failed, can't get source.");
            }
        }
    }

    /// <summary>
    /// A succeeded result.
    /// </summary>
    public sealed class ParserSucceeded : IParserResult {
        public ParserSucceeded(ParserEnvironment environment, IEnumerable<Token> source) {
            this.Environment = environment;
            this.Source = source;
        }

        public Boolean IsSuccessful => true;

        public ParserInput ToInput() => new ParserInput(Environment, Source);

        public ParserEnvironment Environment { get; }

        public IEnumerable<Token> Source { get; }

        public static ParserSucceeded Create(ParserEnvironment environment, IEnumerable<Token> source) =>
        new ParserSucceeded(environment, source);

        public static ParserSucceeded<R> Create<R>(R result, ParserEnvironment environment, IEnumerable<Token> source) =>
        new ParserSucceeded<R>(result, environment, source);
    }

    /// <summary>
    /// A parser result with content.
    /// </summary>
    public interface IParserResult<out R> : IParserResult {
        R Result { get; }
    }

    public sealed class ParserFailed<R> : IParserResult<R> {
        public ParserInput ToInput() {
            throw new InvalidOperationException("Parser failed, can't construct input.");
        }

        public Boolean IsSuccessful => false;

        public R Result {
            get {
                throw new NotSupportedException("Parser failed, can't get result.");
            }
        }

        public ParserEnvironment Environment {
            get {
                throw new NotSupportedException("Parser failed, can't get environment.");
            }
        }

        public IEnumerable<Token> Source {
            get {
                throw new NotSupportedException("Parser failed, can't get source.");
            }
        }
    }

    public sealed class ParserSucceeded<R> : IParserResult<R> {
        public ParserSucceeded(R result, ParserEnvironment environment, IEnumerable<Token> source) {
            this.Result = result;
            this.Environment = environment;
            this.Source = source;
        }

        public ParserInput ToInput() => new ParserInput(Environment, Source);

        public Boolean IsSuccessful => true;

        public R Result { get; }

        public ParserEnvironment Environment { get; }

        public IEnumerable<Token> Source { get; }
    }

}
